const { Client } = require('@elastic/elasticsearch');

const esClient = new Client({
  node: process.env.ELASTICSEARCH_NODE || 'http://localhost:9200',
});

// Test connection
esClient.ping()
  .then(() => console.log('Connected to Elasticsearch'))
  .catch((err) => console.error('Elasticsearch connection failed:', err.message));

module.exports = esClient;
