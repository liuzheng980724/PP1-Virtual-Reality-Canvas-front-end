using UnityEngine;

[CreateAssetMenu]
public class ExampleAppSettings : ScriptableObject
{
    [SerializeField]
    public string[] productionAppIDs; // = "208b6cb9-0a01-4887-92d8-f8ff9e8b2452";

    [SerializeField]
    public string schemeID = "example-fire-training";

    [SerializeField]
    public string task1ID = "read-expiry-date";

    [SerializeField]
    public string task2ID = "remove-safety-pin";

    [SerializeField]
    public string task3ID = "test-extinguisher";

    [SerializeField]
    public string task4ID = "put-out-fire";

    [SerializeField]
    public string task5ID = "multichoice-question";

    [SerializeField]
    public string extinguisherExpiryDate = "2024";

    [SerializeField]
    public float timeToPutOutFire = 2.0f;

    [SerializeField]
    public float endOfTaskDelay = 2.0f;

    /// <summary>
    /// When testing in editor this pairing code needs to be manually
    /// </summary>
    [Header("Development values")]
    [Header("When testing in editor this pairing code needs to be manually")]
    [SerializeField]
    public string editorPairingCode = "58658029";
}