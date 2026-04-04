const axios = require("axios");
const dns = require("dns");
const net = require("net");

const isPrivateIpv4 = (ip) => {
  const parts = ip.split(".").map(Number);

  if (parts.length !== 4 || parts.some((part) => Number.isNaN(part) || part < 0 || part > 255)) {
    return false;
  }

  return (
    parts[0] === 10 ||
    parts[0] === 127 ||
    (parts[0] === 169 && parts[1] === 254) ||
    (parts[0] === 172 && parts[1] >= 16 && parts[1] <= 31) ||
    (parts[0] === 192 && parts[1] === 168) ||
    (parts[0] === 100 && parts[1] >= 64 && parts[1] <= 127) ||
    parts[0] === 0
  );
};

const isPrivateIpv6 = (ip) => {
  const normalizedIp = ip.toLowerCase();

  return (
    normalizedIp === "::1" ||
    normalizedIp === "::" ||
    normalizedIp.startsWith("fc") ||
    normalizedIp.startsWith("fd") ||
    normalizedIp.startsWith("fe80:")
  );
};

const isDisallowedIpAddress = (ip) => {
  if (net.isIP(ip) === 4) {
    return isPrivateIpv4(ip);
  }

  if (net.isIP(ip) === 6) {
    return isPrivateIpv6(ip);
  }

  return false;
};

const validateImageUrl = async (imageUrl) => {
  let parsedUrl;

  try {
    parsedUrl = new URL(imageUrl);
  } catch (error) {
    throw new Error("imageUrl must be a valid URL");
  }

  if (!["http:", "https:"].includes(parsedUrl.protocol)) {
    throw new Error("imageUrl must use http or https");
  }

  if (parsedUrl.username || parsedUrl.password) {
    throw new Error("imageUrl must not include credentials");
  }

  const hostname = parsedUrl.hostname.toLowerCase();

  if (hostname === "localhost" || hostname.endsWith(".localhost")) {
    throw new Error("imageUrl hostname is not allowed");
  }

  if (net.isIP(hostname) && isDisallowedIpAddress(hostname)) {
    throw new Error("imageUrl IP address is not allowed");
  }

  if (!net.isIP(hostname)) {
    const addresses = await dns.promises.lookup(hostname, { all: true, verbatim: true });

    if (!addresses.length) {
      throw new Error("imageUrl hostname could not be resolved");
    }

    if (addresses.some(({ address }) => isDisallowedIpAddress(address))) {
      throw new Error("imageUrl hostname resolves to a disallowed IP address");
    }
  }

  return parsedUrl.toString();
};

const convertImageUrlToBase64 = async (imageUrl) => {
  if (!imageUrl || typeof imageUrl !== "string") {
    throw new Error("imageUrl must be a non-empty string");
  }

  const validatedImageUrl = await validateImageUrl(imageUrl);

  const response = await axios.get(validatedImageUrl, {
    responseType: "arraybuffer",
    maxRedirects: 0,
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