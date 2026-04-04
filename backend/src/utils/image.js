const axios = require("axios");

const convertImageUrlToBase64 = async (imageUrl) => {
  if (!imageUrl || typeof imageUrl !== "string") {
    throw new Error("imageUrl must be a non-empty string");
  }

  const response = await axios.get(imageUrl, {
    responseType: "arraybuffer",
  });

  const base64 = Buffer.from(response.data, "binary").toString("base64");

  const contentType = response.headers["content-type"] || "image/jpeg";

  return {
    base64,
    mimeType: contentType,
  };
};

module.exports = {
  convertImageUrlToBase64,
};