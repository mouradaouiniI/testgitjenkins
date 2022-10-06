using Photon.Pun;
using Photon.Realtime;
using System;
using ExitGames.Client.Photon;

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class gamemanager : MonoBehaviourPunCallbacks
{
 
    public GameObject guessscreen;
    public GameObject scorescreen;
    public GameObject winnerscreen;
    public TextMeshProUGUI winnertxt;
    public int team1score;
    public int team2score;
    public GameObject rightguesspannel;
    public GameObject wrongguesspannel;
    int k;
    public TextMeshProUGUI roundstext;
    public TextMeshProUGUI team1scoretext;
    public TextMeshProUGUI team2scoretext;
    public Slider team1slider;
    public Slider team2slider;
    public Button[] imagesButton;
    public PhotonView view;
    ExitGames.Client.Photon.Hashtable playerdata = new ExitGames.Client.Photon.Hashtable();
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.LocalPlayer.TagObject = 70;
        PhotonNetwork.AutomaticallySyncScene = true;
        StartCoroutine(StartCountdown());
        Player[] players = PhotonNetwork.PlayerList;
        if (PlayerPrefs.GetInt("player") == players.Length)
        {
            PlayerPrefs.SetInt("player", 0);
        }
        PhotonNetwork.SetMasterClient(players[PlayerPrefs.GetInt("player")]);

        Debug.Log(PhotonNetwork.LocalPlayer.NickName);

        // GameObject View= PhotonNetwork.Instantiate("view", transform.position, transform.rotation);
        view = GetComponent<PhotonView>();
        if (view.IsMine) { 
        PlayerPrefs.SetInt("round", PlayerPrefs.GetInt("round") + 1);
    }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public IEnumerator StartCountdown(int countdownValue = 5)
    {
        currCountdownValue = countdownValue;
        while (currCountdownValue > 0)
        {
            countdowntext.SetText(currCountdownValue.ToString());
            yield return new WaitForSeconds(1.0f);
            currCountdownValue--;

        }
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            countdowntext.SetText("");
            title.SetText("you are mimicking");
            yield return new WaitForSeconds(2.0f);
            mimickscreen.SetActive(true);
        }
        else
        {
            countdowntext.SetText("");
            title.SetText("you are guessing");
            yield return new WaitForSeconds(2.0f);
            guessscreen.SetActive(true);
        }


        generateimages();
        currCountdownValue = 10;
        while (currCountdownValue != -1)
        {

            countdowntextingameM.SetText(currCountdownValue.ToString());
            countdowntextingameG.SetText(currCountdownValue.ToString());
            yield return new WaitForSeconds(1.0f);
            currCountdownValue--;

        }
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
        {

            if (EventSystem.current.currentSelectedGameObject!=null)
            {
                if (EventSystem.current.currentSelectedGameObject.gameObject.GetComponent<Image>().sprite == mimicimage.gameObject.GetComponent<Image>().sprite)
                {
                    playerdata["score"] = 10;
                    rightguesspannel.SetActive(true);
                       team1score = 10;
                    scoreholders();
                }
                else
                {
                    wrongguesspannel.SetActive(true);
                    Debug.Log("falseguess");
                    team1score = 0;
                    playerdata["score"] = 0;
                    scoreholders();
                }
                PhotonNetwork.SetPlayerCustomProperties(playerdata);
            }
            else
            {
                wrongguesspannel.SetActive(true);
                Debug.Log("falseguess");
                playerdata["score"] = 0;
                scoreholders();
            }
            PhotonNetwork.SetPlayerCustomProperties(playerdata);
        }
        else
        {
            playerdata["score"] = 10;
            PhotonNetwork.SetPlayerCustomProperties(playerdata);
            scoreholders();
        }
        yield return new WaitForSeconds(4.0f);

        roundstext.SetText("Round " + PlayerPrefs.GetInt("round").ToString() + "/8");
        scorescreen.SetActive(true);
        scoreholders();
        yield return new WaitForSeconds(2.0f);
        scoreholders();

        if (PlayerPrefs.GetInt("round") == 8)
        {
            winnerscreen.SetActive(true);
            if(PlayerPrefs.GetInt("scoreT1")> PlayerPrefs.GetInt("scoreT2"))
            {
                winnertxt.SetText("TEAM 1 WON");
            }
            else
            if (PlayerPrefs.GetInt("scoreT2") > PlayerPrefs.GetInt("scoreT1"))
            {
                winnertxt.SetText("TEAM 2 WON");
            }
            else
            {
                winnertxt.SetText("Draw");

            }

        }
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            if (PlayerPrefs.GetInt("round") != 8)
            {
                nextbutton.SetActive(true);
            }
        }

    }
 
    public void scoreholders()
    {
        Player[] players = PhotonNetwork.PlayerList;

        team1scoretext.SetText(players[0].NickName + " && " + players[1].NickName );

        updatescore(players[0],team1slider,"scoreT1");
        updatescore(players[1],team1slider, "scoreT1");
        updatescore(players[2], team2slider, "scoreT2");
        updatescore(players[3], team2slider, "scoreT2");

    }

    void updatescore(Player player, Slider slider,string team)
    {

        if (player.CustomProperties.ContainsKey("score"))
        {
            slider.value = PlayerPrefs.GetInt(team);
            slider.value += (int)player.CustomProperties["score"];
            PlayerPrefs.SetInt(team, (int)slider.value);
        }
    }


    public void generateimages()
    {
     
        for (int i = 0; i <= imagesparent.childCount - 1; i++)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                int n;
                int k;
                n =UnityEngine.Random.Range(0, 255);
                k =UnityEngine.Random.Range(0, 6);

                view.RPC("sendrpc", RpcTarget.AllBuffered, n, k, i);

            }



        }
    }


    [PunRPC]
    void sendrpc(int l, int m, int i)
    {

        imagesparent.GetChild(i).gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/img_" + l.ToString());

        mimicimage.gameObject.GetComponent<Image>().sprite = imagesparent.GetChild(m).gameObject.GetComponent<Image>().sprite;


    }
    public void selectimage()
    {
        for (int i = 0; i <= imagesparent.childCount - 1; i++)
        {
            imagesparent.GetChild(i).GetChild(0).gameObject.GetComponent<Image>().color = new Color32(217, 217, 217, 255);

        }

        EventSystem.current.currentSelectedGameObject.transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.gray;

    }
    public void home()
    {
        PhotonNetwork.LoadLevel(0);
    }

    public void nextround()
    {


        PlayerPrefs.SetInt("player", PlayerPrefs.GetInt("player") + 1);
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            PhotonNetwork.LoadLevel(2);
        }
        else
        {
            PhotonNetwork.LoadLevel(1);
        }

    }
}
