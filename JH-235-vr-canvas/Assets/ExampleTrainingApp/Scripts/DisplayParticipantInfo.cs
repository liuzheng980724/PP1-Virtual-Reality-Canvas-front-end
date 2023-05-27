using Snobal.Library;
using TMPro;
using UnityEngine;

public class DisplayParticipantInfo : MonoBehaviour
{
    [SerializeField]
    private GameObject content;

    [SerializeField]
    private TextMeshProUGUI nameText;

    public void Show(Participant participant)
    {
        content.SetActive(true);
        nameText.text = participant.firstName;
    }

    public void Hide()
    {
        content.SetActive(false);
    }
}