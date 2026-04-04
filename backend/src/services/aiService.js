const { GoogleGenerativeAI } = require("@google/generative-ai");
const { convertImageUrlToBase64 } = require("../utils/image");

const parseJsonFromModelResponse = (text) => {
  const cleaned = text.trim();

  const fencedMatch = cleaned.match(/```(?:json)?\s*([\s\S]*?)\s*```/i);
  const jsonCandidate = fencedMatch ? fencedMatch[1].trim() : cleaned;

  try {
    return JSON.parse(jsonCandidate);
  } catch {
    const start = cleaned.indexOf("{");
    const end = cleaned.lastIndexOf("}");
    if (start !== -1 && end !== -1 && end > start) {
      return JSON.parse(cleaned.slice(start, end + 1));
    }
    throw new Error("Model response is not valid JSON");
  }
};

const generateProductDetailsFromImage = async (imageUrl) => {
  try {
    if (!process.env.GEMINI_API_KEY) {
      throw new Error("Missing GEMINI_API_KEY environment variable");
    }

    const genAI = new GoogleGenerativeAI(process.env.GEMINI_API_KEY);
    const model = genAI.getGenerativeModel({
      model: "gemini-2.5-flash",
    });

    const { base64, mimeType } = await convertImageUrlToBase64(imageUrl);
    const prompt = `
  You are an e-commerce assistant.

  Given a product image, generate:
  1. Product name (short, clear)
  2. Product description (2-3 sentences)

  Return JSON format:
  {
    "name": "...",
    "description": "..."
  }
  `;

    const result = await model.generateContent([
      {
        inlineData: {
          mimeType,
          data: base64,
        },
      },
      prompt,
    ]);

    const text = result.response.text();

    const parsed = parseJsonFromModelResponse(text);

    return parsed;
  } catch (error) {
    console.error("AI error:", error);
    throw new Error(
      `Failed to generate product info from image: ${error.message}`
    );
  }
};

module.exports = {
  generateProductDetailsFromImage,
};