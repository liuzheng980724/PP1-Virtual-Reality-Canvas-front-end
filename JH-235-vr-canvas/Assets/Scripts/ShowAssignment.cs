using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;


public class QuizDetails
{
    public int id;
    public string title;
    public Question[] questions;
}

public class Question
{
    public string question;
    public Answer[] answers;
}

public class Answer
{
    public string answer;
    public bool is_correct;
}

public class ShowAssignment : MonoBehaviour
{
    public RectTransform assignmentButtonContainer;
    public RectTransform quizButtonContainer;
    public Text assignmentQuizText;
    public float padding = 10f;

    private static readonly HttpClient client = new HttpClient();
    private static readonly string apiUrl = "https://rmit.instructure.com/api/v1";
    private static readonly string accessToken = "9595~XcCTUtdh6v2Fe1DS4ip4MKrzK4DgR4YOBUvcfmkXYKo7zKm3sHBWCyoPxqd2bSGL";
    private static readonly int courseId = 70814;

    async void Start()
    {
        await DisplayAssignmentsAndQuizzes();
    }

    private async Task DisplayAssignmentsAndQuizzes()
    {
        string assignmentsText = await GetAssignmentsText(courseId);
        string[] assignmentNames = assignmentsText.Split('\n');

        foreach (string assignmentName in assignmentNames)
        {
            if (!string.IsNullOrWhiteSpace(assignmentName))
            {
                CreateAssignmentButton(assignmentName);
            }
        }

        List<QuizDetails> quizzes = await FetchQuizzes(courseId);

        foreach (QuizDetails quiz in quizzes)
        {
            if (!string.IsNullOrWhiteSpace(quiz.title))
            {
                CreateQuizButton(quiz.title, quiz.id);
            }
        }
    }

    private void CreateAssignmentButton(string assignmentName)
    {
        CreateButton(assignmentName, assignmentButtonContainer, () => HandleAssignmentButtonClick(assignmentName));
    }

    private void CreateQuizButton(string quizName, int quizId)
    {
        CreateButton(quizName, quizButtonContainer, () => HandleQuizButtonClick(quizId));
    }

    private void CreateButton(string buttonName, RectTransform container, Action buttonClickAction)
    {
        GameObject buttonObject = new GameObject(buttonName);
        buttonObject.transform.SetParent(container);

        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.localPosition = new Vector3(0f, -padding, 0f);

        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(() => buttonClickAction());

        Text buttonTextComponent = buttonObject.AddComponent<Text>();
        buttonTextComponent.text = buttonName;
        buttonTextComponent.alignment = TextAnchor.MiddleLeft;
        buttonTextComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Add additional styling and positioning as desired

        // Adjust the size of the button
        rectTransform.sizeDelta = new Vector2(200f, 30f);
        rectTransform.transform.localScale = new Vector3(1, 1, 1);

        // Increase the padding for the next button
        padding += 30f;
    }

    private void HandleAssignmentButtonClick(string assignmentName)
    {
        // Implement the desired functionality when an assignment button is clicked
        Debug.Log("Clicked assignment: " + assignmentName);
    }

    private async void HandleQuizButtonClick(int quizId)
    {
        QuizDetails quizDetails = await GetQuizDetails(quizId);

        if (quizDetails != null)
        {
            // Display the quiz details (questions, answers, etc.)
            Debug.Log($"Quiz: {quizDetails.title}");

            foreach (Question question in quizDetails.questions)
            {
                Debug.Log($"Question: {question.question}");

                foreach (Answer answer in question.answers)
                {
                    Debug.Log($"Answer: {answer.answer}");
                }
            }
        }
    }

        private static async Task<string> GetAssignmentsText(int courseId)
    {
        string assignmentsText = "";

        string url = $"{apiUrl}/courses/{courseId}/assignments";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        HttpResponseMessage response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            JArray json = JArray.Parse(responseBody);

            foreach (JToken token in json)
            {
                string assignmentName = token["name"].ToString();
                assignmentsText += $"- {assignmentName}\n";
            }
        }
        else
        {
            Debug.LogError($"Error: {response.StatusCode} - {response.ReasonPhrase}");
        }

        return assignmentsText;
    }

    private async Task<QuizDetails> GetQuizDetails(int quizId)
    {
        string url = $"{apiUrl}/courses/{courseId}/quizzes/{quizId}";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        HttpResponseMessage response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            QuizDetails quizDetails = JsonConvert.DeserializeObject<QuizDetails>(responseBody);
            return quizDetails;
        }
        else
        {
            Debug.LogError($"Error: {response.StatusCode} - {response.ReasonPhrase}");
            return null;
        }
    }

    private async Task<List<QuizDetails>> FetchQuizzes(int courseId)
    {
        List<QuizDetails> quizzes = new List<QuizDetails>();

        string url = $"{apiUrl}/courses/{courseId}/quizzes";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        HttpResponseMessage response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            JArray json = JArray.Parse(responseBody);

            foreach (JToken token in json)
            {
                QuizDetails quiz = token.ToObject<QuizDetails>();
                quizzes.Add(quiz);
            }
        }
        else
        {
            Debug.LogError($"Error: {response.StatusCode} - {response.ReasonPhrase}");
        }

        return quizzes;
    }
}

