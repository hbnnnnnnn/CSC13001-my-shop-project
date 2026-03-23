const cloudinary = require("../config/cloudinary.js");
const streamifier = require("streamifier");

const uploadImage = (fileBuffer) => {
    return new Promise((resolve, reject) => {
        const stream = cloudinary.uploader.upload_stream(
            {
                folder: "products",
            },
            (error, result) => {
                if (error) {
                    return reject(error);
                }
                resolve(result.secure_url); // return image url
            }
        );

        // convert buffer to stream then pipe to cloudinary
        streamifier.createReadStream(fileBuffer).pipe(stream);
    });
};

module.exports = { uploadImage };