const { mergeTypeDefs, mergeResolvers } = require('@graphql-tools/merge');
const { loadFilesSync } = require('@graphql-tools/load-files');
const path = require('path');

// 1. Quét tất cả các file có đuôi .js đứng trong thư mục schema/
const typesArray = loadFilesSync(path.join(__dirname, './schema'), { extensions: ['js'] });

// 2. Quét tất cả các file có đuôi .js đứng trong thư mục resolvers/
const resolversArray = loadFilesSync(path.join(__dirname, './resolvers'), { extensions: ['js'] });

// 3. Hàn tất cả lại thành 1 cục duy nhất
const typeDefs = mergeTypeDefs(typesArray);
const resolvers = mergeResolvers(resolversArray);

// 4. Xuất ra cho file server.js dùng
module.exports = {
    typeDefs,
    resolvers
};
