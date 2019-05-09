pragma solidity >=0.4.21 <0.6.0;

contract Betting {
    address payable public owner;
    uint public lastWinnerNumber;
    uint public numberOfBets;
    uint public totalSlots = 3;
    address[] public players;

    mapping(uint => address[]) public numberToPlayers;
    mapping(address => uint) public playerToNumber;

    constructor (uint _totalSlots) public {
        owner = msg.sender;
        if (_totalSlots > 0) totalSlots = _totalSlots;
    }

    modifier validBet(uint betNumber) {
        require(playerToNumber[msg.sender] == 0, "player can bet only once");
        require(numberOfBets < totalSlots, "total bet exceeded");
        require(betNumber >= 1 && betNumber <= 10, "choosen number must be in range 1-10");
        _;
    }

    function bet(uint betNumber) public validBet(betNumber) {
        playerToNumber[msg.sender] = betNumber;
        players.push(msg.sender);
        numberToPlayers[betNumber].push(msg.sender);
        numberOfBets += 1;
        if(numberOfBets >= totalSlots) {
            distributePrizes();
        }
    }

    function distributePrizes() internal {
        uint winnerNumber = generateRandomNumber();
        lastWinnerNumber = winnerNumber;
        reset();
    }

    function generateRandomNumber() internal view returns (uint) {
        return (block.number % 10 + 1);
    }

    function reset() internal {
        for (uint i = 1; i <= 10; i++) {
            numberToPlayers[i].length = 0;
        }

        for (uint j = 0; j < players.length; j++) {
            playerToNumber[players[j]] = 0;
        }

        players.length = 0;
        numberOfBets = 0;
    }

    function kill() public {
        require(msg.sender == owner, "only owner can kill the contract");
        selfdestruct(owner);
    }
}