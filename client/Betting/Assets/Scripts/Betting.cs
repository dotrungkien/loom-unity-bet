using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Loom.Unity3d;
using UnityEngine;
using UnityEngine.UI;

public class Betting : MonoBehaviour
{
    public TextAsset bettingABI;
    public TextAsset bettingAddress;

    public Text lastWinnerNumber;
    public Text myAddress;
    public Text connectionStatus;

    public Button[] numbers;

    private byte[] privateKey;
    private byte[] publicKey;
    private Address from;
    private EvmContract contract;

    async void Start()
    {
        Connect();
        contract = await GetContract();
        await UpdateLastWinNumber();
    }

    void Connect()
    {
        connectionStatus.text = "Connecting";
        string privateKeyHex = PlayerPrefs.GetString("privateKeyHex", "");
        if (privateKeyHex != "")
        {
            privateKey = CryptoUtils.HexStringToBytes(privateKeyHex);
        }
        else
        {
            privateKey = CryptoUtils.GeneratePrivateKey();
            privateKeyHex = CryptoUtils.BytesToHexString(privateKey);
            PlayerPrefs.SetString("privateKeyHex", privateKeyHex);
        }

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
        return new EvmContract(client, contractAddr, from, abi);

    }

    public async Task UpdateLastWinNumber()
    {
        int lastWinNum = await contract.StaticCallSimpleTypeOutputAsync<int>("lastWinnerNumber");
        lastWinnerNumber.text = "" + lastWinNum;
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

}
