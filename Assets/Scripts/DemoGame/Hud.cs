
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
namespace DemoGame
{
    public class Hud : MonoBehaviour
    {
        public TroopSpawner PlayerSpawner;
        public TroopSpawner EnemySpawner;

        [Space(15)]

        public Button PlayerSpawnButton;
        public Button PlayerTroopUpgradeButton;

        [Space(15)]

        public Button EnemySpawnButton;
        public Button EnemyTroopUpgradeButton;

        [Space(15)]
        public TextMeshProUGUI PlayerTroopCount;
        public TextMeshProUGUI EnemyTroopCount;

        [Space(15)]
        public GameObject GameScreen;
        public GameObject StartScreen;
        public GameObject LoadingScreen;
        public Button StartGameButton;

        [Space(15)]
        public TextMeshProUGUI PlayerDoubleDamageIndicator;
        public TextMeshProUGUI EnemyDoubleDamageIndicator;

        [Space(15)]
        public TextMeshProUGUI PlayerUpgradeTierText;
        public TextMeshProUGUI EnemyUpgradeTierText;

        private void Awake()
        {
            GameManager.OnConfigLoaded += OnGameConfigLoaded;
            StartGameButton.onClick.AddListener(() => StartGameButtonListener());
            PlayerSpawner.OnTroopUpdated += DisplayPlayerCount;
            EnemySpawner.OnTroopUpdated += DisplayEnemyCount;

            PlayerSpawnButton.onClick.AddListener(() => PlayerSpawner.SpawnTroop());
            EnemySpawnButton.onClick.AddListener(() => EnemySpawner.SpawnTroop());

            PlayerTroopUpgradeButton.onClick.AddListener(() => PlayerSpawner.UpgradeTroopSpawner());
            EnemyTroopUpgradeButton.onClick.AddListener(() => EnemySpawner.UpgradeTroopSpawner());

            PlayerSpawner.OnDoubleDamageUpdated += UpdatePlayerIndicator;
            EnemySpawner.OnDoubleDamageUpdated += UpdateEnemyIndicator;

            PlayerSpawner.OnTierUpdated += DisplayPlayerUpgradeTier;
            EnemySpawner.OnTierUpdated += DisplayEnemyUpgradeTier;
        }

        private void StartGameButtonListener()
        {
            GameScreen.SetActive(true);
            StartScreen.SetActive(false);
        }

        private void OnGameConfigLoaded()
        {
            StartScreen.SetActive(true);
            LoadingScreen.SetActive(false);
        }
        private void DisplayPlayerCount(int current, int max)
        {
            PlayerTroopCount.text = "Player:" + current + "/" + max;
        }

        private void DisplayEnemyCount(int current, int max)
        {
            EnemyTroopCount.text = "Enemy:" + current + "/" + max;
        }

        private void UpdatePlayerIndicator(bool value)
        {
            PlayerDoubleDamageIndicator.gameObject.SetActive(value);
        }
        private void UpdateEnemyIndicator(bool value)
        {
            EnemyDoubleDamageIndicator.gameObject.SetActive(value);
        }

        private void DisplayPlayerUpgradeTier(int tier)
        {
            PlayerUpgradeTierText.text = "upgrade tier:" + tier;
        }
        private void DisplayEnemyUpgradeTier(int tier)
        {
            EnemyUpgradeTierText.text = "upgrade tier:" + tier;
        }
    }
}