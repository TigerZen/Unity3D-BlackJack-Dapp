using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;
using Nethereum.JsonRpc.UnityClient;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Contracts;
using System;
using UnityEngine.SceneManagement;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using System.IO;
using Assets.Tweens.Scripts.Common.Tweens;
using System.Collections.Generic;
using System.Linq;

public class BlackJack : MonoBehaviour
{
    #region 變數
    private string accountAddress;
    private string accountPrivateKey; //遊戲開始輸入私鑰
    public int TANCoins;
    public int BetTCoins;
    public int SlotResult;
    public int MineGasAmount = 300000; //輸入挖礦GasAmount預設300000
    public int MineGasPrice = 1000000000; //輸入挖礦GasPrice預設1GWEI

    public Text AddressTAN;
    public Text CopyMainAddressText;
    public Text TANAmount;
    public Text WINAmount;
    public Text TotalPrizeAmount;
    private decimal TANBalance;
    public decimal TotalPrize;
    public Text TransactionText;
    public GameObject EnterPkeyPanel;
    public GameObject LoadingPanel;
    public GameObject BlockBG;
    public GameObject NoEnoughTAN;

    public int StartResult;  //開局開牌結果(result => 0=都沒有/1=都有平手/2=只有玩家21點/3=只有莊家21點)
    public int PlayerPoint;  //玩家點數總和
    public int BankerPoint;  //莊家點數總和
    public int PlayerCardsAmount;  //玩家卡牌數
    public int BankerCardsAmount;  //莊家卡牌數
    public Text PlayerTotalPoint;  //玩家點數總和顯示文字
    public Text BankerTotalPoint;  //莊家家點數總和顯示文字
    public int[] PlayersCardNO;
    public int[] BankersCardNO;
    public GameObject PlayersCardGroup;
    public GameObject BankersCardGroup;
    public GameObject[] PlayersCard;
    public GameObject[] BankersCard;
    public Sprite[] PokerCardSprit;

    public GameObject DealPanel;
    public GameObject PlayPanel;
    public GameObject PlacebetsPanel;
    public GameObject WINBGPanel;
    public GameObject WINPanel;
    public GameObject PUSHPanel;
    public GameObject BUSTPanel;
    public GameObject P_BlackJackPanel;
    public GameObject B_BlackJackPanel;
    public GameObject StandPanel;
    public GameObject ShowCardsPanel;
    public GameObject HitPanel;
    public GameObject RestartGamePanel;
    #endregion
    
    #region 智能合約
    //-----以下為智能合約資料----
    private string _urlMain = "https://testnet-rpc.tangerine-network.io";  //TAN 測試鏈網路
    //private string _urlMain = "https://mainnet-rpc.tangerine-network.io";  //TAN 主鏈網路
    private DeployContractTransactionBuilder contractTransactionBuilder = new DeployContractTransactionBuilder();
    private static string ContractAddressBJ21 = "0x4233808F20dbA600cA97Dd2f1F99E504c95E245C"; //TAN 測試鏈合約地址
    private static string ABIBJ21 = @"[{""constant"":false,""inputs"":[],""name"":""Banker_ShowCard"",""outputs"":[],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""constant"":false,""inputs"":[],""name"":""Player_Stand"",""outputs"":[],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""manager"",""outputs"":[{""name"":"""",""type"":""address""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[{""name"":""_address"",""type"":""address""}],""name"":""inquireBanker"",""outputs"":[{""name"":"""",""type"":""uint8[]""},{""name"":"""",""type"":""uint8""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[{""name"":""_address"",""type"":""address""}],""name"":""inquireBankerCard2"",""outputs"":[{""name"":""card2"",""type"":""uint8""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[],""name"":""inquireETH"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":true,""inputs"":[{""name"":""_address"",""type"":""address""}],""name"":""inquirePlayer"",""outputs"":[{""name"":"""",""type"":""uint8[]""},{""name"":"""",""type"":""uint8""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":false,""inputs"":[],""name"":""Player_Hit"",""outputs"":[],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""constant"":false,""inputs"":[],""name"":""startGame"",""outputs"":[],""payable"":true,""stateMutability"":""payable"",""type"":""function""},{""constant"":true,""inputs"":[{""name"":""_address"",""type"":""address""}],""name"":""inquireBankerCard1"",""outputs"":[{""name"":""card1"",""type"":""uint8""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},{""constant"":false,""inputs"":[],""name"":""withdraw_allETH"",""outputs"":[],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""constant"":false,""inputs"":[],""name"":""EndGame"",""outputs"":[],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""constant"":false,""inputs"":[{""name"":""_new_manager"",""type"":""address""}],""name"":""transferownership"",""outputs"":[],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[],""payable"":true,""stateMutability"":""payable"",""type"":""constructor""},{""payable"":true,""stateMutability"":""payable"",""type"":""fallback""},{""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""player"",""type"":""address""},{""indexed"":false,""name"":""result"",""type"":""uint8""}],""name"":""startResult"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""player"",""type"":""address""},{""indexed"":false,""name"":""playerCard1"",""type"":""uint256""},{""indexed"":false,""name"":""playerCard2"",""type"":""uint8""}],""name"":""playerStartCards"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""player"",""type"":""address""},{""indexed"":false,""name"":""count"",""type"":""uint256""},{""indexed"":false,""name"":""newCard"",""type"":""uint8""}],""name"":""playerHit"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""player"",""type"":""address""},{""indexed"":false,""name"":""playerCard"",""type"":""uint8[]""}],""name"":""playTotal"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""player"",""type"":""address""},{""indexed"":false,""name"":""bankerCard1"",""type"":""uint8""}],""name"":""bankCard1"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""player"",""type"":""address""},{""indexed"":false,""name"":""bankerCard2"",""type"":""uint8""}],""name"":""bankCard2"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""player"",""type"":""address""},{""indexed"":false,""name"":""bankerCard"",""type"":""uint8[]""}],""name"":""bankTotal"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":false,""name"":""result"",""type"":""uint8""},{""indexed"":false,""name"":""playerCard"",""type"":""uint8""},{""indexed"":false,""name"":""bankerCard"",""type"":""uint8""}],""name"":""gameOver"",""type"":""event""}]";
    private Contract ContractBJ21;

    public BlackJack()
    {
        this.ContractBJ21 = new Contract(null, ABIBJ21, ContractAddressBJ21);
    }
    //-----以上為智能合約資料----
    #endregion

    #region 初始化
    void Start()
    {
        EnterPkeyPanel.gameObject.SetActive(true);
        PlacebetsPanel.gameObject.SetActive(false);
    }

    public void AfterEnterPkey()
    {
        LoadingPanel.gameObject.SetActive(false);
        BlockBG.gameObject.SetActive(false);
        NoEnoughTAN.gameObject.SetActive(false);
        DealPanel.gameObject.SetActive(true);
        PlayPanel.gameObject.SetActive(false);
        for (int i = 0; i < 11; i++)
        {
            PlayersCard[i].gameObject.GetComponent<Image>().sprite = null;
            BankersCard[i].gameObject.GetComponent<Image>().sprite = null;
            PlayersCard[i].gameObject.SetActive(false);
            BankersCard[i].gameObject.SetActive(false);
            PlayersCardNO[i] = 0;
            BankersCardNO[i] = 0;
        }
        PlayersCardGroup.GetComponent<GridLayoutGroup>().constraintCount = 11;
        BankersCardGroup.GetComponent<GridLayoutGroup>().constraintCount = 11;
        PlacebetsPanel.gameObject.SetActive(false);
        WINPanel.gameObject.SetActive(false);
        WINBGPanel.gameObject.SetActive(false);
        PUSHPanel.gameObject.SetActive(false);
        BUSTPanel.gameObject.SetActive(false);
        P_BlackJackPanel.gameObject.SetActive(false);
        B_BlackJackPanel.gameObject.SetActive(false);
        StandPanel.gameObject.SetActive(false);
        ShowCardsPanel.gameObject.SetActive(false);
        HitPanel.gameObject.SetActive(false);
        RestartGamePanel.gameObject.SetActive(false);
        WINAmount.text = "";
        PlayerTotalPoint.text = "";
        BankerTotalPoint.text = "";
        importAccountFromPrivateKey();
    }
    #endregion

    #region 重啟及其他功能
    public void ReStartGame()
    {
        LoadingPanel.gameObject.SetActive(false);
        BlockBG.gameObject.SetActive(false);
        NoEnoughTAN.gameObject.SetActive(false);
        //ButtonBack.gameObject.SetActive(false);
        DealPanel.gameObject.SetActive(true);
        PlayPanel.gameObject.SetActive(false);
        PlacebetsPanel.gameObject.SetActive(true);
        WINPanel.gameObject.SetActive(false);
        WINBGPanel.gameObject.SetActive(false);
        PUSHPanel.gameObject.SetActive(false);
        BUSTPanel.gameObject.SetActive(false);
        P_BlackJackPanel.gameObject.SetActive(false);
        B_BlackJackPanel.gameObject.SetActive(false);
        StandPanel.gameObject.SetActive(false);
        ShowCardsPanel.gameObject.SetActive(false);
        HitPanel.gameObject.SetActive(false);
        RestartGamePanel.gameObject.SetActive(false);
        WINAmount.text = "";
        PlayerTotalPoint.text = "";
        BankerTotalPoint.text = "";
        for (int i = 0; i < 11; i++)
        {
            PlayersCard[i].gameObject.GetComponent<Image>().sprite = null;
            BankersCard[i].gameObject.GetComponent<Image>().sprite = null;
            PlayersCard[i].gameObject.SetActive(false);
            BankersCard[i].gameObject.SetActive(false);
            PlayersCardNO[i] = 0;
            BankersCardNO[i] = 0;
        }
        PlayersCardGroup.GetComponent<GridLayoutGroup>().constraintCount = 11;
        BankersCardGroup.GetComponent<GridLayoutGroup>().constraintCount = 11;
        TransactionText.text = "";
        ReloadBalance();
        ReloadPollBalance();
    }

    public void ShowDescription(int ShowType)
    {
        if(ShowType == 1)
        {
            TransactionText.text = "Banker's chip";
        }
        else if (ShowType == 2)
        {
            TransactionText.text = "Player's chip";
        }
        else if (ShowType == 3)
        {
            TransactionText.text = "";
        }

    }

    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion

    #region 私鑰類函數
    public void EnterPkeyStart(Text enterText)
    {
        accountPrivateKey = enterText.text;
    }

    public void ImportPkey()
    {
        InputField PasswordInput1 = GameObject.Find("InputField_Pkey").GetComponent<InputField>();
        accountPrivateKey = PasswordInput1.text;
        AfterEnterPkey();
    }

    public void CreatePkey()
    {
        EthECKey ecKey = EthECKey.GenerateKey();
        accountPrivateKey = ecKey.GetPrivateKey();
        CopyMainPrivateKey();
        AfterEnterPkey();
    }

    public void CopyMainPrivateKey()   //複製私鑰
    {
        TextEditor text2Editor = new TextEditor();
        text2Editor.text = accountPrivateKey.RemoveHexPrefix();
        text2Editor.OnFocus();
        text2Editor.Copy();   //--將私鑰複製到剪貼版
        TransactionText.text = "Private key has been copied！";
    }

    public void ReloadSceneGo()
    {
        LoadingPanel.gameObject.SetActive(true);
        StartCoroutine(ReloadSceneGoWait(2.0f));
    }

    public IEnumerator ReloadSceneGoWait(float waittimes)
    {
        yield return new WaitForSeconds(waittimes);
        SceneManager.LoadScene("FastSlot");
    }

    public void CopyMainAddress()   //複製地址
    {
        CopyMainAddressText.text = accountAddress;
        TextEditor text2Editor = new TextEditor();
        text2Editor.text = accountAddress;
        text2Editor.OnFocus();
        text2Editor.Copy();   //--將地址複製到剪貼版
        TransactionText.text = "Address has been copied！";
    }

    public void importAccountFromPrivateKey()
    {
        // 這裡是把我們的私鑰轉為公鑰地址的函數
        if (accountPrivateKey == null || accountPrivateKey == "")
        {
            TransactionText.text = "Private key not loaded";
            LoadingPanel.gameObject.SetActive(true);
        }
        else
        {
            try
            {
                var address = Nethereum.Signer.EthECKey.GetPublicAddress(accountPrivateKey);
                // 把address轉換取得的地址字串轉給accountAdress
                accountAddress = address;
                AddressTAN.text = accountAddress;  //將取得的地址顯示在螢幕上
                Debug.Log("私鑰轉換成地址成功:" + address);
                checkPlayerCards(1);
                ReloadBalance();
                ReloadPollBalance();
            }
            catch (Exception e)
            {
                TransactionText.text = "Private key not loaded";
                LoadingPanel.gameObject.SetActive(true);
                Debug.Log("error" + e);
            }
        }
    }
    #endregion

    #region 執行啟動BJ的合約
    //----以下是啟動BJ的函數---(調用執行函數)
    public void PlayBJ(int BetAmount)
    {
        TransactionText.text = "";
        PlacebetsPanel.gameObject.SetActive(false);
        WINPanel.gameObject.SetActive(false);
        PUSHPanel.gameObject.SetActive(false);
        BUSTPanel.gameObject.SetActive(false);
        P_BlackJackPanel.gameObject.SetActive(false);
        B_BlackJackPanel.gameObject.SetActive(false);
        if (TANBalance < BetAmount)
        {
            NoEnoughTAN.gameObject.SetActive(true);
        }
        else
        {
            if (TotalPrize < BetAmount * 2)
            {
                TransactionText.text = "Banker not enough TAN！";
            }
            else
            {
                LoadingPanel.gameObject.SetActive(true);
                NoEnoughTAN.gameObject.SetActive(false);
                StartCoroutine(PlayBJGo(BetAmount));
                BetTCoins = BetAmount;
                WINAmount.text = "";
                PlayerTotalPoint.text = "";
                BankerTotalPoint.text = "";
                TANAmount.text = Convert.ToInt16(Math.Floor(TANBalance - BetTCoins)).ToString("#0");
                BlockBG.gameObject.SetActive(true);
                for (int i = 0; i < 11; i++)
                {
                    PlayersCard[i].gameObject.GetComponent<Image>().sprite = null;
                    BankersCard[i].gameObject.GetComponent<Image>().sprite = null;
                    PlayersCard[i].gameObject.SetActive(false);
                    BankersCard[i].gameObject.SetActive(false);
                    PlayersCardNO[i] = 0;
                    BankersCardNO[i] = 0;
                }
            }
        }
    }

    public IEnumerator PlayBJGo(int BetAmount)
    {
        var transactionInput = CreatePlayBJInput(
            accountAddress,
            new HexBigInteger(MineGasAmount), //GasAmount
            new HexBigInteger(MineGasPrice), //GasPrice / GWEI
            new HexBigInteger(Nethereum.Util.UnitConversion.Convert.ToWei(BetAmount, 18)) //賭注金額 / TAN
        );
        Debug.Log("你已經開始玩BJ，等待系統確認...");
        TransactionText.text = "Play Blackjack " + BetAmount + "X...";
        var transactionSignedRequest = new TransactionSignedUnityRequest(_urlMain, accountPrivateKey, accountAddress);
        yield return transactionSignedRequest.SignAndSendTransaction(transactionInput);
        if (transactionSignedRequest.Exception == null)
        {
            Debug.Log("Transfered tx created: " + transactionSignedRequest.Result);
            checkPlayBJTx(transactionSignedRequest.Result, (cb) => {
                Debug.Log("Blackjack 開啟！");
                TransactionText.text = "Blackjack Start！";
                LoadingPanel.gameObject.SetActive(false);
                if(StartResult == 0)
                {
                    checkPlayerCards(1);
                    DealPanel.gameObject.SetActive(false);
                    PlayPanel.gameObject.SetActive(true);
                    StandPanel.gameObject.SetActive(true);
                    ShowCardsPanel.gameObject.SetActive(false);
                    HitPanel.gameObject.SetActive(true);
                    RestartGamePanel.gameObject.SetActive(false);
                }
                else if (StartResult == 1)
                {
                    PUSHPanel.gameObject.SetActive(true);
                    checkPlayerCards(1);
                }
                else if (StartResult == 2)
                {
                    P_BlackJackPanel.gameObject.SetActive(true);
                    checkPlayerCards(1);
                }
                else if (StartResult == 3)
                {
                    B_BlackJackPanel.gameObject.SetActive(true);
                    checkPlayerCards(1);
                }
            });
        }
        else
        {
            Debug.Log("Error transfering: " + transactionSignedRequest.Exception.Message);
        }
    }

    public TransactionInput CreatePlayBJInput(
        string addressFrom,
        HexBigInteger gas = null,
        HexBigInteger gasPrice = null,
        HexBigInteger valueAmount = null
    )
    {
        var function = GetPlayBJGoFunction();
        return function.CreateTransactionInput(
            addressFrom, gas, gasPrice, valueAmount
        );
    }

    public Function GetPlayBJGoFunction()  //----執行ABI
    {
        return ContractBJ21.GetFunction("startGame");
    }

    public void checkPlayBJTx(string txHash, Action<bool> callback) // 這個函數是監聽交易是否成功的功能
    {
        StartCoroutine(CheckPlayBJIsMined(
            _urlMain,
            txHash,
            (cb) => {
                Debug.Log("本交易已經完成");
                callback(true);
            }
        ));
    }

    public IEnumerator CheckPlayBJIsMined(             //------監聽交易在區塊練是否確認成功
        string url, string txHash, System.Action<bool> callback
    )
    {
        var mined = false;
        var tries = 3600;
        while (!mined)
        {
            if (tries > 0)
            {
                tries = tries - 1;
            }
            else
            {
                mined = true;
                Debug.Log("Performing last try..");
            }
            Debug.Log("Checking receipt for: " + txHash);
            var receiptRequest = new EthGetTransactionReceiptUnityRequest(url);
            yield return receiptRequest.SendRequest(txHash);
            if (receiptRequest.Exception == null)
            {
                if (receiptRequest.Result != null)
                {
                    string txLogs1 = receiptRequest.Result.Logs[0]["data"].ToString();  //玩家開局牌面兩張牌
                    string txLogs2 = receiptRequest.Result.Logs[1]["data"].ToString();  //莊家家開局牌面第一張牌
                    string txLogs3 = receiptRequest.Result.Logs[2]["data"].ToString();  //開局結果
                    var txLogsHex1 = txLogs1.RemoveHexPrefix();
                    var txLogsHex2 = txLogs2.RemoveHexPrefix();
                    var txLogsHex3 = txLogs3.RemoveHexPrefix();
                    string PlayerCard1 = txLogsHex1.Substring(0, 64);
                    string PlayerCard2 = txLogsHex1.Substring(64, 64);
                    PlayersCardNO[0] = int.Parse(PlayerCard1, System.Globalization.NumberStyles.AllowHexSpecifier);
                    PlayersCardNO[1] = int.Parse(PlayerCard2, System.Globalization.NumberStyles.AllowHexSpecifier);
                    string BankersCard0 = txLogsHex2.Substring(0, 64);
                    BankersCardNO[0] = int.Parse(BankersCard0, System.Globalization.NumberStyles.AllowHexSpecifier);
                    string StartResultGet = txLogsHex3.Substring(0, 64);
                    StartResult = int.Parse(StartResultGet, System.Globalization.NumberStyles.AllowHexSpecifier);
                    Debug.Log("BJ Start 1:玩家第1張牌:" + PlayerCard1 + "/玩家第2張牌:" + PlayerCard2 + "/" + txLogsHex3);
                    Debug.Log("BJ Start 2:莊家第1張牌" + BankersCardNO[0] + "/" + txLogsHex2);
                    Debug.Log("BJ Start 3:開局結果:" + StartResult + "/" + txLogsHex3);
                    var txType = "mined";
                    if (txType == "mined")
                    {
                        mined = true;
                        callback(mined);
                    }
                    else
                    {
                        mined = false;
                        callback(mined);
                    }
                }
            }
            else
            {
                Debug.Log("Error checking receipt: " + receiptRequest.Exception.Message);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
    //----以上是啟動BJ的函數---(調用執行函數)
    #endregion

    #region 執行補牌的合約Hit
    //----以下是執行補牌的函數---(調用執行函數)
    public void PlayerHit()
    {
        BlockBG.gameObject.SetActive(true);
        TransactionText.text = "Player Hit";
        StartCoroutine(PlayerHitGo());
    }

    public IEnumerator PlayerHitGo()
    {
        var transactionInput = CreatePlayerHitInput(
            accountAddress,
            new HexBigInteger(MineGasAmount), //GasAmount
            new HexBigInteger(MineGasPrice), //GasPrice / GWEI
            new HexBigInteger(0) //TAN
        );
        Debug.Log("玩家補牌，等待系統確認...");
        TransactionText.text = "Player Hit...";
        var transactionSignedRequest = new TransactionSignedUnityRequest(_urlMain, accountPrivateKey, accountAddress);
        yield return transactionSignedRequest.SignAndSendTransaction(transactionInput);
        if (transactionSignedRequest.Exception == null)
        {
            Debug.Log("Transfered tx created: " + transactionSignedRequest.Result);
            checkPlayerHitTx(transactionSignedRequest.Result, (cb) => {
                Debug.Log("補牌完成！");
                TransactionText.text = "Player hit done.";
                LoadingPanel.gameObject.SetActive(false);
                BlockBG.gameObject.SetActive(false);
                checkPlayerCards(1);
            });
        }
        else
        {
            Debug.Log("Error transfering: " + transactionSignedRequest.Exception.Message);
        }
    }

    public TransactionInput CreatePlayerHitInput(
        string addressFrom,
        HexBigInteger gas = null,
        HexBigInteger gasPrice = null,
        HexBigInteger valueAmount = null
    )
    {
        var function = GetPlayerHitGoFunction();
        return function.CreateTransactionInput(
            addressFrom, gas, gasPrice, valueAmount
        );
    }

    public Function GetPlayerHitGoFunction()  //----執行ABI
    {
        return ContractBJ21.GetFunction("Player_Hit");
    }

    public void checkPlayerHitTx(string txHash, Action<bool> callback) // 這個函數是監聽交易是否成功的功能
    {
        StartCoroutine(CheckPlayerHitIsMined(
            _urlMain,
            txHash,
            (cb) => {
                Debug.Log("本交易已經完成");
                callback(true);
            }
        ));
    }

    public IEnumerator CheckPlayerHitIsMined(             //------監聽交易在區塊練是否確認成功
        string url, string txHash, System.Action<bool> callback
    )
    {
        var mined = false;
        var tries = 3600;
        while (!mined)
        {
            if (tries > 0)
            {
                tries = tries - 1;
            }
            else
            {
                mined = true;
                Debug.Log("Performing last try..");
            }
            Debug.Log("Checking receipt for: " + txHash);
            var receiptRequest = new EthGetTransactionReceiptUnityRequest(url);
            yield return receiptRequest.SendRequest(txHash);
            if (receiptRequest.Exception == null)
            {
                if (receiptRequest.Result != null)
                {
                    string txLogs1 = receiptRequest.Result.Logs[0]["data"].ToString();
                    var txLogsHex1 = txLogs1.RemoveHexPrefix();
                    string PlayerCardSortGet = txLogsHex1.Substring(0, 64);
                    int PlayerCardSort = int.Parse(PlayerCardSortGet, System.Globalization.NumberStyles.AllowHexSpecifier) - 1;
                    string PlayerCardNOGet = txLogsHex1.Substring(64, 64);
                    PlayersCardNO[PlayerCardSort] = int.Parse(PlayerCardNOGet, System.Globalization.NumberStyles.AllowHexSpecifier);
                    Debug.Log("Hit結果1:" + txLogsHex1);
                    Debug.Log("Hit結果2:玩家補第" + PlayerCardSortGet + "張牌/牌號為:" + PlayerCardNOGet);
                    var txType = "mined";
                    if (txType == "mined")
                    {
                        mined = true;
                        callback(mined);
                    }
                    else
                    {
                        mined = false;
                        callback(mined);
                    }
                }
            }
            else
            {
                Debug.Log("Error checking receipt: " + receiptRequest.Exception.Message);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
    //----以上是執行補牌的函數---(調用執行函數)
    #endregion

    #region 執行停止補牌的合約Stand
    //----以下是執行停止補牌的函數---(調用執行函數)
    public void PlayerStand(int SType)
    {
        TransactionText.text = "";
        BlockBG.gameObject.SetActive(true);
        StartCoroutine(PlayerStandGo(SType));
        P_BlackJackPanel.gameObject.SetActive(false);
        B_BlackJackPanel.gameObject.SetActive(false);
    }

    public IEnumerator PlayerStandGo(int SType)
    {
        var transactionInput = CreatePlayerStandInput(
            accountAddress,
            SType,
            new HexBigInteger(MineGasAmount), //GasAmount
            new HexBigInteger(MineGasPrice), //GasPrice / GWEI
            new HexBigInteger(0) //TAN
        );
        if (SType == 1)
        {
            Debug.Log("玩家停止補牌，等待系統確認...");
            TransactionText.text = "Player Stand...";
        }
        else if (SType == 2)
        {
            Debug.Log("莊家開牌補牌，等待系統確認...");
            TransactionText.text = "Banker Show Cards...";
        }
        else if (SType == 3)
        {
            Debug.Log("結束遊戲莊家洗牌，等待系統確認...");
            TransactionText.text = "Restart Game...";
            WINPanel.gameObject.SetActive(false);
            PUSHPanel.gameObject.SetActive(false);
            BUSTPanel.gameObject.SetActive(false);
            P_BlackJackPanel.gameObject.SetActive(false);
            B_BlackJackPanel.gameObject.SetActive(false);
        }
        var transactionSignedRequest = new TransactionSignedUnityRequest(_urlMain, accountPrivateKey, accountAddress);
        yield return transactionSignedRequest.SignAndSendTransaction(transactionInput);
        if (transactionSignedRequest.Exception == null)
        {
            Debug.Log("Transfered tx created: " + transactionSignedRequest.Result);
            checkPlayerStandTx(transactionSignedRequest.Result, (cb) => {
                Debug.Log("停止補牌完成！");
                LoadingPanel.gameObject.SetActive(false);
                BlockBG.gameObject.SetActive(false);
                if(SType == 1)
                {
                    TransactionText.text = "Player Stand.";
                    PlayerStand(2);
                }
                else if (SType == 2)
                {
                    TransactionText.text = "Banker Show Cards.";
                    checkPlayerCards(2);
                }
                else if (SType == 3)
                {
                    TransactionText.text = "Restart Game.";
                    ReStartGame();
                }
            });
        }
        else
        {
            PlayerStand(SType);
            Debug.Log("Error transfering: " + transactionSignedRequest.Exception.Message);
        }
    }

    public TransactionInput CreatePlayerStandInput(
        string addressFrom,
        int SType,
        HexBigInteger gas = null,
        HexBigInteger gasPrice = null,
        HexBigInteger valueAmount = null
    )
    {
        var function = GetPlayerStandGoFunction(SType);
        return function.CreateTransactionInput(
            addressFrom, gas, gasPrice, valueAmount
        );
    }

    public Function GetPlayerStandGoFunction(int SType)  //----執行ABI
    {
        string STypeString = "";
        if (SType == 1)
        {
            STypeString = "Player_Stand";  //查詢玩家牌面資訊
        }
        else if (SType == 2)
        {
            STypeString = "Banker_ShowCard";  //查詢莊家牌面資訊
        }
        else if (SType == 3)
        {
            STypeString = "EndGame";  //查詢莊家第一張牌
        }
        return ContractBJ21.GetFunction(STypeString);
    }

    public void checkPlayerStandTx(string txHash, Action<bool> callback) // 這個函數是監聽交易是否成功的功能
    {
        StartCoroutine(CheckPlayerStandIsMined(
            _urlMain,
            txHash,
            (cb) => {
                Debug.Log("本交易已經完成");
                callback(true);
            }
        ));
    }

    public IEnumerator CheckPlayerStandIsMined(             //------監聽交易在區塊練是否確認成功
        string url, string txHash, System.Action<bool> callback
    )
    {
        var mined = false;
        var tries = 3600;
        while (!mined)
        {
            if (tries > 0)
            {
                tries = tries - 1;
            }
            else
            {
                mined = true;
                Debug.Log("Performing last try..");
            }
            Debug.Log("Checking receipt for: " + txHash);
            var receiptRequest = new EthGetTransactionReceiptUnityRequest(url);
            yield return receiptRequest.SendRequest(txHash);
            if (receiptRequest.Exception == null)
            {
                if (receiptRequest.Result != null)
                {
                    string txLogs1 = receiptRequest.Result.Logs[0]["data"].ToString();
                    var txLogsHex1 = txLogs1.RemoveHexPrefix();
                    //string PlayerCardSortGet = txLogsHex1.Substring(0, 64);
                    //int PlayerCardSort = int.Parse(PlayerCardSortGet, System.Globalization.NumberStyles.AllowHexSpecifier) - 1;
                    //string PlayerCardNOGet = txLogsHex1.Substring(64, 64);
                    //PlayersCardNO[PlayerCardSort] = int.Parse(PlayerCardNOGet, System.Globalization.NumberStyles.AllowHexSpecifier);
                    Debug.Log("Hit結果1:" + txLogsHex1);
                    //Debug.Log("Hit結果2:玩家補第" + PlayerCardSortGet + "張牌/牌號為:" + PlayerCardNOGet);
                    var txType = "mined";
                    if (txType == "mined")
                    {
                        mined = true;
                        callback(mined);
                    }
                    else
                    {
                        mined = false;
                        callback(mined);
                    }
                }
            }
            else
            {
                Debug.Log("Error checking receipt: " + receiptRequest.Exception.Message);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
    //----以上是執行停止補牌的函數---(調用執行函數)
    #endregion

    #region 取得玩家或莊家牌面及點數
    //--------------取得玩家牌面及點數---(查詢)
    public void checkPlayerCards(int CheckType)
    {
        StartCoroutine(GetPlayerCards(CheckType, 1));
        TransactionText.text = "";
    }

    public void checkBankerCards(int CheckType)
    {
        StartCoroutine(GetPlayerCards(CheckType, 2));
    }

    public void checkBankerCard1(int CheckType)
    {
        StartCoroutine(GetPlayerCards(CheckType, 3));
    }

    public void checkBankerCard2(int CheckType)
    {
        StartCoroutine(GetPlayerCards(CheckType, 4));
    }

    public IEnumerator GetPlayerCards(int CheckType, int inquireType)
    {
        var GetPlayerCardsRequest = new EthCallUnityRequest(_urlMain);
        var GetPlayerCardsInput = CreateGetPlayerCardsInput(inquireType);
        yield return GetPlayerCardsRequest.SendRequest(GetPlayerCardsInput, Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest());
        if (GetPlayerCardsRequest.Exception == null)
        {
            var PlayerCardsHex = GetPlayerCardsRequest.Result.RemoveHexPrefix();
            Debug.Log("取得牌面及點數: " + PlayerCardsHex);
            PlacebetsPanel.gameObject.SetActive(false);
            DealPanel.gameObject.SetActive(false);
            PlayPanel.gameObject.SetActive(true);
            if (inquireType == 1)
            {
                string CheckStartGet = PlayerCardsHex.Substring(124, 4);
                int CheckStart = int.Parse(CheckStartGet, System.Globalization.NumberStyles.AllowHexSpecifier);
                if (CheckStart == 0)
                {
                    PlacebetsPanel.gameObject.SetActive(true);
                    DealPanel.gameObject.SetActive(true);
                    PlayPanel.gameObject.SetActive(false);
                }
                else
                {
                    string PlayerPointGet = PlayerCardsHex.Substring(64, 64);
                    string PlayerCardsAmountGet = PlayerCardsHex.Substring(128, 64);
                    PlayerPoint = int.Parse(PlayerPointGet, System.Globalization.NumberStyles.AllowHexSpecifier);
                    PlayerCardsAmount = int.Parse(PlayerCardsAmountGet, System.Globalization.NumberStyles.AllowHexSpecifier);
                    PlayerTotalPoint.text = PlayerPoint.ToString();
                    if(PlayerCardsAmount > 5)
                    {
                        PlayersCardGroup.GetComponent<GridLayoutGroup>().constraintCount = 5;
                    }
                    else
                    {
                        PlayersCardGroup.GetComponent<GridLayoutGroup>().constraintCount = 11;
                    }
                    for (int i = 0; i < PlayerCardsAmount; i++)
                    {
                        PlayersCard[i].gameObject.SetActive(true);
                        PlayersCardNO[i] = int.Parse(PlayerCardsHex.Substring(192 + i * 64, 64), System.Globalization.NumberStyles.AllowHexSpecifier);
                        PlayersCard[i].gameObject.GetComponent<Image>().sprite = PokerCardSprit[PlayersCardNO[i]];
                    }
                    if (PlayerPoint > 21)
                    {
                        BUSTPanel.gameObject.SetActive(true);
                        checkBankerCards(1);
                        StandPanel.gameObject.SetActive(false);
                        ShowCardsPanel.gameObject.SetActive(false);
                        HitPanel.gameObject.SetActive(false);
                        RestartGamePanel.gameObject.SetActive(true);
                    }
                    else if (PlayerPoint == 21)
                    {
                        if(PlayerCardsAmount == 2)
                        {
                            WINBGPanel.gameObject.SetActive(false);
                            P_BlackJackPanel.gameObject.SetActive(true);
                            StandPanel.gameObject.SetActive(false);
                            ShowCardsPanel.gameObject.SetActive(false);
                            HitPanel.gameObject.SetActive(false);
                            RestartGamePanel.gameObject.SetActive(true);
                            checkBankerCards(2);
                        }
                        else
                        {
                            WINBGPanel.gameObject.SetActive(true);
                            StandPanel.gameObject.SetActive(true);
                            ShowCardsPanel.gameObject.SetActive(false);
                            HitPanel.gameObject.SetActive(false);
                            RestartGamePanel.gameObject.SetActive(false);
                            checkBankerCards(2);
                        }
                    }
                    else
                    {
                        StandPanel.gameObject.SetActive(true);
                        ShowCardsPanel.gameObject.SetActive(false);
                        HitPanel.gameObject.SetActive(true);
                        RestartGamePanel.gameObject.SetActive(false);
                        checkBankerCards(1);
                    }
                }
            }
            else if (inquireType == 2)
            {
                string CheckFirstGet = PlayerCardsHex.Substring(124, 4);
                int CheckFirst = int.Parse(CheckFirstGet, System.Globalization.NumberStyles.AllowHexSpecifier);
                if (CheckFirst == 0)
                {
                    checkBankerCard1(1);
                }
                else
                {
                    string BankerPointGet = PlayerCardsHex.Substring(64, 64);
                    string BankerCardsAmountGet = PlayerCardsHex.Substring(128, 64);
                    BankerPoint = int.Parse(BankerPointGet, System.Globalization.NumberStyles.AllowHexSpecifier);
                    BankerCardsAmount = int.Parse(BankerCardsAmountGet, System.Globalization.NumberStyles.AllowHexSpecifier);
                    BankerTotalPoint.text = BankerPoint.ToString();
                    if (BankerCardsAmount > 5)
                    {
                        BankersCardGroup.GetComponent<GridLayoutGroup>().constraintCount = 5;
                    }
                    else
                    {
                        BankersCardGroup.GetComponent<GridLayoutGroup>().constraintCount = 11;
                    }
                    for (int i = 0; i < BankerCardsAmount; i++)
                    {
                        BankersCard[i].gameObject.SetActive(true);
                        BankersCardNO[i] = int.Parse(PlayerCardsHex.Substring(192 + i * 64, 64), System.Globalization.NumberStyles.AllowHexSpecifier);
                        BankersCard[i].gameObject.GetComponent<Image>().sprite = PokerCardSprit[BankersCardNO[i]];
                    }
                    StandPanel.gameObject.SetActive(false);
                    ShowCardsPanel.gameObject.SetActive(false);
                    HitPanel.gameObject.SetActive(false);
                    RestartGamePanel.gameObject.SetActive(true);
                    if (BankerPoint > 21)
                    {
                        if(PlayerPoint <= 21)
                        {
                            WINPanel.gameObject.SetActive(true);
                            WINBGPanel.gameObject.SetActive(true);
                            StandPanel.gameObject.SetActive(false);
                            ShowCardsPanel.gameObject.SetActive(false);
                            HitPanel.gameObject.SetActive(false);
                            RestartGamePanel.gameObject.SetActive(true);
                        }
                        else
                        {
                            PUSHPanel.gameObject.SetActive(true);
                            StandPanel.gameObject.SetActive(false);
                            ShowCardsPanel.gameObject.SetActive(false);
                            HitPanel.gameObject.SetActive(false);
                            RestartGamePanel.gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        if (BankerPoint > PlayerPoint && BankerPoint != 21)
                        {
                            BUSTPanel.gameObject.SetActive(true);
                            StandPanel.gameObject.SetActive(false);
                            ShowCardsPanel.gameObject.SetActive(false);
                            HitPanel.gameObject.SetActive(false);
                            RestartGamePanel.gameObject.SetActive(true);
                        }
                        else
                        {
                            if (BankerPoint == PlayerPoint)
                            {
                                PUSHPanel.gameObject.SetActive(true);
                                StandPanel.gameObject.SetActive(false);
                                ShowCardsPanel.gameObject.SetActive(false);
                                HitPanel.gameObject.SetActive(false);
                                RestartGamePanel.gameObject.SetActive(true);
                            }
                            else if (BankerPoint == 21 && BankerPoint != PlayerPoint)
                            {
                                if (BankerCardsAmount == 2)
                                {
                                    B_BlackJackPanel.gameObject.SetActive(true);
                                    StandPanel.gameObject.SetActive(false);
                                    ShowCardsPanel.gameObject.SetActive(false);
                                    HitPanel.gameObject.SetActive(false);
                                    RestartGamePanel.gameObject.SetActive(true);
                                }
                                else
                                {
                                    B_BlackJackPanel.gameObject.SetActive(true);
                                    StandPanel.gameObject.SetActive(false);
                                    ShowCardsPanel.gameObject.SetActive(false);
                                    HitPanel.gameObject.SetActive(false);
                                    RestartGamePanel.gameObject.SetActive(true);
                                }
                            }
                            else
                            {
                                if (PlayerPoint > 21)
                                {
                                    BUSTPanel.gameObject.SetActive(true);
                                    StandPanel.gameObject.SetActive(false);
                                    ShowCardsPanel.gameObject.SetActive(false);
                                    HitPanel.gameObject.SetActive(false);
                                    RestartGamePanel.gameObject.SetActive(true);
                                }
                                else
                                {
                                    if(PlayerPoint == 21 && PlayerCardsAmount == 2)
                                    {
                                        P_BlackJackPanel.gameObject.SetActive(true);
                                        WINBGPanel.gameObject.SetActive(true);
                                        StandPanel.gameObject.SetActive(false);
                                        ShowCardsPanel.gameObject.SetActive(false);
                                        HitPanel.gameObject.SetActive(false);
                                        RestartGamePanel.gameObject.SetActive(true);
                                    }
                                    else
                                    {
                                        WINPanel.gameObject.SetActive(true);
                                        WINBGPanel.gameObject.SetActive(true);
                                        StandPanel.gameObject.SetActive(false);
                                        ShowCardsPanel.gameObject.SetActive(false);
                                        HitPanel.gameObject.SetActive(false);
                                        RestartGamePanel.gameObject.SetActive(true);
                                    }
                                }
                            }
                        }
                    }
                    BlockBG.gameObject.SetActive(false);
                }
            }
            else if (inquireType == 3)
            {
                string BankerCard1Get = PlayerCardsHex.Substring(0, 64);
                BankersCardNO[0] = int.Parse(BankerCard1Get, System.Globalization.NumberStyles.AllowHexSpecifier);
                BankersCard[0].gameObject.SetActive(true);
                BankersCard[0].gameObject.GetComponent<Image>().sprite = PokerCardSprit[BankersCardNO[0]];
                int BankerCard1Point = BankersCardNO[0] % 13;
                if (BankerCard1Point == 0 || BankerCard1Point >= 10)
                {
                    BankerPoint = 10;
                }
                else
                {
                    BankerPoint = BankerCard1Point;
                }
                BankerTotalPoint.text = BankerPoint.ToString();
                checkBankerCard2(1);
                BlockBG.gameObject.SetActive(false);
            }
            else if (inquireType == 4)
            {
                string BankerCard2Get = PlayerCardsHex.Substring(60, 4);
                BankersCardNO[1] = int.Parse(BankerCard2Get, System.Globalization.NumberStyles.AllowHexSpecifier);
                if (BankersCardNO[1] != 0)
                {
                    int BankerCard2Point = BankersCardNO[1] % 13;
                    if (BankerCard2Point == 0 || BankerCard2Point >= 10)
                    {
                        BankerPoint += 10;
                    }
                    else
                    {
                        BankerPoint += BankerCard2Point;
                    }
                    if (BankerPoint == PlayerPoint)
                    {
                        PUSHPanel.gameObject.SetActive(true);
                    }
                    StandPanel.gameObject.SetActive(false);
                    ShowCardsPanel.gameObject.SetActive(true);
                    HitPanel.gameObject.SetActive(false);
                    RestartGamePanel.gameObject.SetActive(false);
                }
                BankerTotalPoint.text = BankerPoint.ToString();
                BankersCard[1].gameObject.SetActive(true);
                BankersCard[1].gameObject.GetComponent<Image>().sprite = PokerCardSprit[BankersCardNO[1]];
                if(StartResult > 0)
                {
                    StandPanel.gameObject.SetActive(false);
                    ShowCardsPanel.gameObject.SetActive(false);
                    HitPanel.gameObject.SetActive(false);
                    RestartGamePanel.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            yield return new WaitForSeconds(2);
            checkPlayerCards(CheckType);
        }
    }

    public CallInput CreateGetPlayerCardsInput(int inquireType)
    {
        var function = GetPlayerCardsFunction(inquireType);
        return function.CreateCallInput(accountAddress);
    }
    public Function GetPlayerCardsFunction(int inquireType)
    {
        string inquireString = "";
        if (inquireType == 1)
        {
            inquireString = "inquirePlayer";  //查詢玩家牌面資訊
        }
        else if (inquireType == 2)
        {
            inquireString = "inquireBanker";  //查詢莊家牌面資訊
        }
        else if (inquireType == 3)
        {
            inquireString = "inquireBankerCard1";  //查詢莊家第一張牌
        }
        else if (inquireType == 4)
        {
            inquireString = "inquireBankerCard2";  //查詢莊家第二張牌
        }
        return ContractBJ21.GetFunction(inquireString);
    }
    //--------------取得玩家牌面及點數---(查詢)
    #endregion

    #region 餘額查詢
    //--------------取得玩家地址TAN餘額---(查詢)
    public void ReloadBalance()
    {
        StartCoroutine(getAccountBalance(accountAddress, (balance) => {     //執行檢查主網ETH的餘額
            TANAmount.text = Convert.ToInt16(Math.Floor(balance)).ToString("#0");
            TANBalance = balance;
            Debug.Log(balance);
            BlockBG.gameObject.SetActive(false);
            LoadingPanel.gameObject.SetActive(false);
        }));
    }

    public static IEnumerator getAccountBalance(string address, System.Action<decimal> callback)
    {
        //--取得主網ETH餘額
        var getBalanceRequest = new EthGetBalanceUnityRequest("https://testnet-rpc.tangerine-network.io");
        yield return getBalanceRequest.SendRequest(address, Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest());
        if (getBalanceRequest.Exception == null)
        {
            var balance = getBalanceRequest.Result.Value;
            yield return new WaitForSeconds(1);
            callback(Nethereum.Util.UnitConversion.Convert.FromWei(balance, 18));
        }
        else
        {
            throw new System.InvalidOperationException("取得TAN餘額失敗");
        }
    }
    //--------------取得玩家地址TAN餘額---(查詢)
    //--------------取得合約地址TAN餘額---(查詢)
    public void ReloadPollBalance()
    {
        StartCoroutine(getAccountBalance(ContractAddressBJ21, (balance) => {     //執行檢查主網ETH的餘額
            TotalPrizeAmount.text = Convert.ToInt16(Math.Floor(balance)).ToString("#0");
            TotalPrize = balance;
            Debug.Log(balance);
        }));
    }
    //--------------取得合約地址TAN餘額---(查詢)
    #endregion

    #region Update
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    #endregion
}
