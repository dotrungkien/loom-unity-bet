const { readFileSync } = require("fs")
const LoomTruffleProvider = require("loom-truffle-provider")

const chainId = "default"
const writeUrl = "http://127.0.0.1:46658/rpc"
const readUrl = "http://127.0.0.1:46658/query"
const privateKey = readFileSync("./priv", "utf-8")

const loomTruffleProvider = new LoomTruffleProvider(chainId, writeUrl, readUrl, privateKey)

module.exports = {
  networks: {
    loom_dapp_chain: {
      provider: loomTruffleProvider,
      network_id: "*"
    }
  },

  mocha: {},

  compilers: {
    solc: {}
  }
}
