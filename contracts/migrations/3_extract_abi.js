const fs = require("fs")
const path = require("path")
const contracts = path.resolve(__dirname, "../build/contracts/")
const unityAbis = path.resolve(__dirname, "../../client/Betting/Assets/Contracts/")
const loomNetwork = "13654820909954"

module.exports = async function(deployer, network, accounts) {
  let builtContracts = fs.readdirSync(contracts)
  builtContracts.forEach((contract) => {
    if (contract === "Migrations.json") return
    const name = contract.split(".")[0]
    let json = JSON.parse(fs.readFileSync(path.resolve(contracts, contract)))
    let { abi, networks } = json
    fs.writeFileSync(path.resolve(unityAbis, contract), JSON.stringify(json.abi))
    fs.writeFileSync(path.resolve(unityAbis, name + "Address.txt"), networks[loomNetwork].address)
  })
}
