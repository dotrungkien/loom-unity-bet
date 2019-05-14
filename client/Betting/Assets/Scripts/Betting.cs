using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Loom.Unity3d;
using UnityEngine;
using UnityEngine.UI;

using Loom.Nethereum.ABI.FunctionEncoding;
using Loom.Nethereum.ABI.FunctionEncoding.Attributes;
using Loom.Nethereum.ABI.Model;

public class Betting : MonoBehaviour
{
    public TextAsset bettingABI;
    public TextAsset bettingAddress;

    public Text lastWinnerNumber;
    public Text myAddress;
    public Text betNumber;

    public Button[] numbers;
    public Text[] choosenNumbers;
    public Text[] players;

    private byte[] privateKey;
    private byte[] publicKey;
    private Address from;
    private EvmContract contract;

    async void Start()
    {
        Connect();
        for (int i = 0; i < numbers.Length; i++)
        {
            int j = i;
            numbers[j].onClick.AddListener(async () => await Bet(j + 1));
        }
        for (int i = 0; i < players.Length; i++)
        {
            players[i].text = "";
            choosenNumbers[i].text = "";
        }
        contract = await GetContract();
        await UpdateLastWinNumber();
        await UpdatePlayers();
    }

    void Connect()
    {
        // string privateKeyHex = PlayerPrefs.GetString("privateKeyHex", "");
        // if (privateKeyHex != "")
        // {
        //     privateKey = CryptoUtils.HexStringToBytes(privateKeyHex);
        // }
        // else
        // {
        //     privateKey = CryptoUtils.GeneratePrivateKey();
        //     privateKeyHex = CryptoUtils.BytesToHexString(privateKey);
        //     PlayerPrefs.SetString("privateKeyHex", privateKeyHex);
        // }

        privateKey = CryptoUtils.GeneratePrivateKey();
        publicKey = CryptoUtils.PublicKeyFromPrivateKey(privateKey);
        from = Address.FromPublicKey(publicKey);
        myAddress.text = from.LocalAddressHexString;
    }

    async Task<EvmContract> GetContract()
    {
        var writer = RPCClientFactory.Configure()
            // .WithLogger(Debug.unityLogger)
            .WithWebSocket("ws://127.0.0.1:46657/websocket")
            .Create();

        var reader = RPCClientFactory.Configure()
            // .WithLogger(Debug.unityLogger)
            .WithWebSocket("ws://127.0.0.1:9999/queryws")
            .Create();

        var client = new DAppChainClient(writer, reader)
        {
            // Logger = Debug.unityLogger
        };

        client.TxMiddleware = new TxMiddleware(new ITxMiddlewareHandler[]{
            new NonceTxMiddleware{
                PublicKey = publicKey,
                Client = client
            },
            new SignedTxMiddleware(privateKey)
        });

        string abi = bettingABI.ToString();
        var contractAddr = Address.FromHexString(bettingAddress.ToString());
        EvmContract evmContract = new EvmContract(client, contractAddr, from, abi);
        evmContract.EventReceived += ContractEventReceived;
        return evmContract;
    }

    public async Task UpdateLastWinNumber()
    {
        int lastWinNum = await contract.StaticCallSimpleTypeOutputAsync<int>("lastWinnerNumber");
        lastWinnerNumber.text = "" + lastWinNum;
    }

    public async Task UpdatePlayers()
    {
        int numberOfBets = await contract.StaticCallSimpleTypeOutputAsync<int>("numberOfBets");
        if (numberOfBets > 0)
        {
            for (int i = 0; i < numberOfBets; i++)
            {
                string player = await contract.StaticCallSimpleTypeOutputAsync<string>("players", i);
                players[i].text = player;
                int choosenNumber = await contract.StaticCallSimpleTypeOutputAsync<int>("playerToNumber", player);
                choosenNumbers[i].text = "" + choosenNumber;
            }
        }
    }


    public async Task Bet(int number)
    {
        Debug.Log("bet number " + number);
        await contract.CallAsync("bet", number);
        betNumber.text = "" + number;
        await UpdateLastWinNumber();
        await UpdatePlayers();
    }

    public async Task StaticCallContract(string func)
    {
        if (contract == null)
        {
            throw new Exception("Not signed in!");
        }
        Debug.Log("Calling smart contract...");
        int result = await contract.StaticCallSimpleTypeOutputAsync<int>(func);
        Debug.Log("Smart contract returned: " + result);
    }

    public class OnBetEvent
    {
        [Parameter("address", "from", 1)]
        public int From { get; set; }

        [Parameter("uint256", "betNumber", 2)]
        public int BetNumber { get; set; }
    }

    private async void ContractEventReceived(object sender, EvmChainEventArgs e)
    {
        Debug.LogFormat("Received smart contract event: " + e.EventName);
        if (e.EventName == "OnBet")
        {
            await UpdatePlayers();
            // Debug.Log("On evenbbbbbbbbbbbb");
            // OnBetEvent evt = e.DecodeEventDTO<OnBetEvent>();
            // Debug.Log(evt);
            // Debug.Log("frommmmmmmmmmmmmmm" + evt.From);
            // Debug.Log("bettttttttttttttttttttttt" + evt.BetNumber);
            // Debug.Log("On eventaaaaaaaaaaaaaaaaaaaaaaaaa");
        }
    }

}
