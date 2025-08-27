using UnityEngine;

public class RiskRewardScreen : MonoBehaviour
{
    [SerializeField] private GameObject[] riskCards; 
    [SerializeField] private GameObject[] rewardCards;

    private void Start()
    {
        enabled = false;
    }

    private void OnEnable()
    {
        foreach (Transform _child in transform)
        {
            _child.gameObject.SetActive(true);
        }

        SetActiveCards(true, riskCards);
        SetActiveCards(false, rewardCards);

        // Pause game and unlock cursor
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDisable()
    {
        foreach (Transform _child in transform)
        {
            _child.gameObject.SetActive(false);
        }

        // Resume game and lock cursor
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ChooseRisk(GameObject _chosenRisk)
    {
        SetActiveCards(false, riskCards);
        SetActiveCards(true, rewardCards);
    }

    public void ChooseReward(GameObject _chosenReward)
    {
        enabled = false;
    }

    private void SetActiveCards(bool _enable, GameObject[] _cards)
    {
        Debug.Log(_enable);
        foreach (GameObject _card in _cards)
        {
            _card.SetActive(_enable);
        }
    }
}
