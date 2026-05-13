// Import service — parse .xlsx buffer and upsert products into DB
const XLSX = require("xlsx");
const productRepository = require("../repositories/product.repository.js");
const categoryRepository = require("../repositories/category.repository.js");
const { indexProduct } = require("./search.service.js");
const cacheService = require("../utils/cache.util.js");

// Column names we expect (lowercased for resilient matching)
const REQUIRED_FIELDS = ["sku", "name", "price", "stock"];
const OPTIONAL_FIELDS = ["description", "supplier", "category_name", "images"];
const ALL_FIELDS = [...REQUIRED_FIELDS, ...OPTIONAL_FIELDS];

/**
 * Normalize header keys: trim whitespace and lowercase
 */
function normalizeRow(raw) {
  const normalized = {};
  for (const key of Object.keys(raw)) {
    normalized[key.trim().toLowerCase()] = raw[key];
  }
  return normalized;
}

/**
 * Validate a single normalized row. Returns an error message string or null if valid.
 */
function validateRow(row) {
  for (const field of REQUIRED_FIELDS) {
    if (
      row[field] === undefined ||
      row[field] === null ||
      String(row[field]).trim() === ""
    ) {
      return `Missing required field: ${field}`;
    }
  }

  const price = Number(row.price);
  if (!Number.isInteger(price) || price < 0) {
    return `Invalid price: "${row.price}" (must be a non-negative integer)`;
  }

  const stock = Number(row.stock);
  if (!Number.isInteger(stock) || stock < 0) {
    return `Invalid stock: "${row.stock}" (must be a non-negative integer)`;
  }

  // Validate images URLs if provided
  if (row.images && String(row.images).trim() !== "") {
    const imageUrls = String(row.images)
      .split(",")
      .map((url) => url.trim())
      .filter((url) => url !== "");

    for (const url of imageUrls) {
      try {
        new URL(url); // Validate URL format
      } catch (e) {
        return `Invalid image URL: "${url}" (must be a valid URL)`;
      }
    }
  }

  return null;
}

/**
 * Parse images from comma-separated string into array of URLs
 */
function parseImages(imagesString) {
  if (!imagesString || String(imagesString).trim() === "") {
    return null;
  }

  const urls = String(imagesString)
    .split(",")
    .map((url) => url.trim())
    .filter((url) => url !== "");

  return urls.length > 0 ? urls : null;
}

/**
 * Main import function.
 * @param {Buffer} fileBuffer — the .xlsx file buffer from multer
 * @returns {{ summary, errors, upserted }}
 */
async function importProductsFromExcel(fileBuffer) {
  // 1. Parse workbook
  const workbook = XLSX.read(fileBuffer, { type: "buffer" });
  const sheetName = workbook.SheetNames[0];
  if (!sheetName) {
    throw new Error("The Excel file contains no sheets");
  }

  const rawRows = XLSX.utils.sheet_to_json(workbook.Sheets[sheetName]);
  if (rawRows.length === 0) {
    throw new Error("The Excel sheet is empty (no data rows found)");
  }

  // 2. Normalize and validate each row
  const errors = [];
  const validRows = [];

  for (let i = 0; i < rawRows.length; i++) {
    const rowNumber = i + 2; // Excel row (1-indexed header + 1-indexed data)
    const row = normalizeRow(rawRows[i]);

    const error = validateRow(row);
    if (error) {
      errors.push({ row: rowNumber, sku: row.sku || "", reason: error });
      continue;
    }

    validRows.push({ ...row, _rowNumber: rowNumber });
  }

  if (validRows.length === 0) {
    return {
      summary: {
        total_rows: rawRows.length,
        upserted: 0,
        errors: errors.length,
      },
      errors,
      upserted: [],
    };
  }

  // 3. Resolve category names → category IDs
  const uniqueCategoryNames = [
    ...new Set(
      validRows
        .map((r) => r.category_name)
        .filter(
          (name) =>
            name !== undefined && name !== null && String(name).trim() !== "",
        )
        .map((name) => String(name).trim()),
    ),
  ];

  // Build a name→id map by querying each category
  const categoryMap = {};
  for (const name of uniqueCategoryNames) {
    const category = await categoryRepository.findByName(name);
    if (category) {
      categoryMap[name.toLowerCase()] = category.category_id;
    }
  }

  // Separate rows whose category_name doesn't exist
  const rowsToUpsert = [];
  for (const row of validRows) {
    if (row.category_name && String(row.category_name).trim() !== "") {
      const catName = String(row.category_name).trim();
      const catId = categoryMap[catName.toLowerCase()];
      if (!catId) {
        errors.push({
          row: row._rowNumber,
          sku: row.sku,
          reason: `Category "${catName}" not found`,
        });
        continue;
      }
      row.category_id = catId;
    }
    rowsToUpsert.push(row);
  }

  if (rowsToUpsert.length === 0) {
    return {
      summary: {
        total_rows: rawRows.length,
        upserted: 0,
        errors: errors.length,
      },
      errors,
      upserted: [],
    };
  }

  // 4. Prepare product objects for upsert
  const productsToUpsert = rowsToUpsert.map((row) => ({
    sku: String(row.sku).trim(),
    name: String(row.name).trim(),
    price: Number(row.price),
    stock: Number(row.stock),
    description: row.description ? String(row.description).trim() : null,
    supplier: row.supplier ? String(row.supplier).trim() : null,
    category_id: row.category_id || null,
    images: parseImages(row.images),
  }));

  // 5. Upsert in a single batch
  let upsertedProducts = [];
  try {
    upsertedProducts = await productRepository.upsertMany(productsToUpsert);
  } catch (dbError) {
    throw new Error(`Database error during upsert: ${dbError.message}`);
  }

  // 6. Sync Elasticsearch for each upserted product
  for (const product of upsertedProducts) {
    try {
      const productWithCategory = await productRepository.findByIdWithCategory(
        product.product_id,
      );
      await indexProduct(productWithCategory);
    } catch (esError) {
      console.error(
        `[Import] Failed to index product ${product.product_id} in Elasticsearch:`,
        esError.message,
      );
      // Non-fatal — continue
    }
  }

  // 7. Invalidate product caches
  await cacheService.delByPrefix("products:p:");
  await cacheService.delByPrefix("products:low_stock:");
  await cacheService.delByPrefix("products:top_selling:");
  await cacheService.delByPrefix("report:");

  return {
    summary: {
      total_rows: rawRows.length,
      upserted: upsertedProducts.length,
      errors: errors.length,
    },
    errors,
    upserted: upsertedProducts.map((p) => ({
      product_id: p.product_id,
      sku: p.sku,
      name: p.name,
    })),
  };
}

module.exports = { importProductsFromExcel };

