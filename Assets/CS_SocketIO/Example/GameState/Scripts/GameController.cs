using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private GameObject GameContainer;
    [SerializeField]
    private Transform PlayersContainer;
    [SerializeField]
    private Transform CoinsContainer;

    [SerializeField]
    private GameObject thiefPregab;
    [SerializeField]
    private GameObject richPrefab;
    [SerializeField]
    private GameObject CoinPrefab;
    [SerializeField]
    private GameObject CirclePrefab;

    private GameState State;
    private Dictionary<string, Transform> PlayersToRender;
    private Dictionary<string, Transform> CoinsToRender;
    private Dictionary<string, Transform> CirclesToRender;
    private InputController inputController;


    public event Action OnRemoveFromCircles;
    private void Start()
    {
        inputController = FindObjectOfType<InputController>();
    }
    internal void StartGame(GameState state)
    {
        PlayersToRender = new Dictionary<string, Transform>();
        CoinsToRender = new Dictionary<string, Transform>();
        CirclesToRender = new Dictionary<string, Transform>();
        GameObject.Find("PanelConnect").SetActive(false);
        GameContainer.SetActive(true);


        foreach (Player player in state.Players)
        {
            InstantiatePlayer(player);
        }

        var Socket = NetworkController._Instance.Socket;

        InputController._Instance.onAxisChange += (axis) => { Socket.Emit("move", axis); };
        InputController._Instance.OnSpellCast += () => { Socket.Emit("Cast", state.Players.FirstOrDefault(p =>p.type =="Rich") ); };
        OnRemoveFromCircles += () => {
            Player richPlayer = state.Players.FirstOrDefault(p => p.type == "Rich");
            if (richPlayer != null)
            {
                Socket.Emit("Clear", richPlayer);
            }
        };


        State = state;
        Socket.On("updateState", UpdateState);
    }

    private void InstantiatePlayer(Player player)
    {
        GameObject playerGameObject;
        if (player.type == "Rich")
        {
            playerGameObject = Instantiate(richPrefab, PlayersContainer);
        }
        else
        {
            playerGameObject = Instantiate(thiefPregab, PlayersContainer);
        }
        inputController.Setplayer(playerGameObject.GetComponent<Animator>(), playerGameObject.GetComponent<SpriteRenderer>());
        playerGameObject.transform.position = new Vector2(player.x, player.y);
        playerGameObject.GetComponent<GamePlayer>().Id = player.Id;
        playerGameObject.GetComponent<GamePlayer>().Username = player.Id;

        PlayersToRender[player.Id] = playerGameObject.transform;
    }

    private void UpdateState(string json)
    {
        GameStateData jsonData = JsonUtility.FromJson<GameStateData>(json);
        State = jsonData.State;
    }

    internal void NewPlayer(string id, string username, string type ,int lifes )
    {
            InstantiatePlayer(new Player { Id = id, Username = username });
    }

    void Update()
    {
        if (State != null)
        {
            foreach (Player player in State.Players)
            {
                if (PlayersToRender.ContainsKey(player.Id))
                {
                    PlayersToRender[player.Id].position = new Vector2(player.x, player.y);
                }
                else
                {
                    InstantiatePlayer(player);
                }
              
            }
            if(State.MagicCircles != null)
            {
                foreach (var circle in State.MagicCircles)
                {
                    if(State.Players.FirstOrDefault(p => p.type == "Rich").CanCast)
                    {
                        Debug.Log("circle in" + circle.x + "," + circle.y);
                        InstantiateCircle(circle);
                        
                    }
                }
            }

            var plarersToDelete = PlayersToRender.Where(item => !State.Players.Any(player => player.Id == item.Key)).ToList();
            foreach (var playerItem in plarersToDelete)
            {
                Destroy(playerItem.Value.gameObject);
                PlayersToRender.Remove(playerItem.Key);
            }
            foreach (Coin coin in State.Coins)
            {
                if (CoinsToRender.ContainsKey(coin.Id))
                {
                    CoinsToRender[coin.Id].position = new Vector2(coin.x, coin.y);
                }
                else
                {
                    InstantiateCoin(coin);
                }
            }
            var coinsToDelete = CoinsToRender.Where(item => !State.Coins.Any(coin => coin.Id == item.Key)).ToList();

            foreach (var coinItem in coinsToDelete)
            {
                Destroy(coinItem.Value.gameObject);
                CoinsToRender.Remove(coinItem.Key);
            }

        }
    }
    private void InstantiateCoin(Coin coin)
    {
        GameObject coinGameObject = Instantiate(CoinPrefab, CoinsContainer);
        coinGameObject.transform.position = new Vector2(coin.x, coin.y);
        coinGameObject.GetComponent<GameCoin>().Id = coin.Id;

        CoinsToRender[coin.Id] = coinGameObject.transform;
    }
    private void InstantiateCircle(Circle circle)
    {
        GameObject circleGameObject = Instantiate(CirclePrefab, CoinsContainer);
        circleGameObject.transform.position = new Vector2(circle.x, circle.y);
        CirclesToRender.Add(circle.Id, circleGameObject.transform);
        //circleGameObject.GetComponent<GameCoin>().Id = circle.Id;
    }
}
[Serializable]
public class GameStateData
{
    public GameState State;
}
