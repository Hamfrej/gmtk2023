using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TargetInformationUI : MonoBehaviour
{
    public static TargetInformationUI Instance { get; private set;}
    [SerializeField]
    private TextMeshProUGUI userNameText;
    [SerializeField]
    private TextMeshProUGUI handleText;
    [SerializeField]
    private TextMeshProUGUI descriptonText;
    [SerializeField]
    private TextMeshProUGUI traitsText;

    [SerializeField] private Slider agendaSlider;
    [SerializeField] private TextMeshProUGUI agendaCounter;

    [SerializeField] private Slider boredomSlider;
    [SerializeField] private TextMeshProUGUI boredomCounter;

    [SerializeField] public RawImage avatar;

    Texture2D texture;

    public UserProfile targetUser;

    private void Awake() {
        if ( Instance != null)
        {
            Debug.LogError("There's more than once GameManager! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetTargetUser(UserProfile targetUser)
    {
        this.targetUser = targetUser;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        userNameText.text = targetUser.userName;
        handleText.text = targetUser.userHandle;
        descriptonText.text = targetUser.description;
        traitsText.text = targetUser.GetTraitsForUi();

        texture = Resources.Load("avatars/" + targetUser.avatarResourcePath) as Texture2D;
        avatar.texture = texture;
    }

    public void UpdateBars(int agendaScore, int boredomLevel)
    {
        agendaSlider.value = agendaScore;
        agendaCounter.text  = agendaScore.ToString() + "/6";
        boredomSlider.value = boredomLevel;
        Image boredomFill = boredomSlider.GetComponentInChildren<Image>();
        if (boredomLevel > 0 && boredomLevel <= 5)
        {
            boredomFill.color = Color.green;
        }
        else if ((boredomLevel > 5 && boredomLevel <= 10))
        {
            boredomFill.color = Color.yellow;
        }
        else if (boredomLevel > 10)
        {
            boredomFill.color = Color.red;
        }
        boredomCounter.text  = boredomLevel.ToString() + "/15";
    }
    
}
