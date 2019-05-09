const Betting = artifacts.require("Betting")

module.exports = function(deployer) {
  deployer.deploy(Betting, 3)
}
