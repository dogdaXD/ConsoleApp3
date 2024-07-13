using System;
using System.Text.Json;

class Program
{
    static void Main()
    {
        Console.WriteLine("--- Welcome ---");

        bool exit = false;
        var registration = new Registration();
        var login = new Login();
        var quizManager = new QuizManager();

        while (!exit)
        {
            Console.WriteLine("1. Registration");
            Console.WriteLine("2. Log in");
            Console.WriteLine("3. Exit");

            int choice = GetChoice();

            switch (choice)
            {
                case 1:
                    registration.DoRegistration();
                    break;
                case 2:
                    string loggedInUser = login.DoLogin();
                    if (loggedInUser != null)
                    {
                        DisplayUserMenu(loggedInUser, quizManager);
                    }
                    break;
                case 3:
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please choose again.");
                    break;
            }
        }
    }

    static void DisplayUserMenu(string username, QuizManager quizManager)
    {
        bool logout = false;
        while (!logout)
        {
            Console.WriteLine($"\n--- Welcome, {username} ---");
            Console.WriteLine("1. Start a New Quiz");
            Console.WriteLine("2. View Past Quiz Results");
            Console.WriteLine("3. View Top 20 Scores for a Quiz");
            Console.WriteLine("4. Change Settings");
            Console.WriteLine("5. Logout");

            int choice = GetChoice();

            switch (choice)
            {
                case 1:
                    Console.WriteLine("\nChoose a Quiz Topic:");
                    Console.WriteLine("1. History");
                    Console.WriteLine("2. Geography");
                    Console.WriteLine("3. Biology");
                    Console.WriteLine("4. Mixed Quiz");

                    int quizChoice = GetChoice();

                    switch (quizChoice)
                    {
                        case 1:
                            quizManager.StartQuiz("History", username);
                            break;
                        case 2:
                            quizManager.StartQuiz("Geography", username);
                            break;
                        case 3:
                            quizManager.StartQuiz("Biology", username);
                            break;
                        case 4:
                            quizManager.StartMixedQuiz(username);
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please choose again.");
                            break;
                    }
                    break;
                case 2:
                    quizManager.ViewPastResults(username);
                    break;
                case 3:
                    quizManager.ViewTopScores();
                    break;
                case 4:
                    UserManager.UpdateUserSettings(username);
                    break;
                case 5:
                    logout = true;
                    Console.WriteLine($"Logging out {username}");
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please choose again.");
                    break;
            }
        }
    }

    static int GetChoice()
    {
        int choice;
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out choice) && choice >= 1 && choice <= 5)
            {
                return choice;
            }
            else
            {
                Console.WriteLine("Please enter a number from 1 to 5.");
            }
        }
    }
}

class Registration
{
    private const string usersFile = "users.txt";

    public void DoRegistration()
    {
        Console.WriteLine("\n--- Registration ---");
        Console.WriteLine("INPUT username (letters only):");
        string username = Console.ReadLine()!;

        while (!IsAllLetters(username))
        {
            Console.WriteLine("Username should contain only letters. Please try again.");
            username = Console.ReadLine()!;
        }

        Console.WriteLine("Input password (digits only):");
        string password = Console.ReadLine()!;

        while (!IsAllDigits(password))
        {
            Console.WriteLine("Password should contain only digits. Please try again.");
            password = Console.ReadLine();
        }

        Console.WriteLine("Input date of birth (YYYY-MM-DD):");
        DateTime dateOfBirth;
        while (!DateTime.TryParse(Console.ReadLine(), out dateOfBirth))
        {
            Console.WriteLine("Invalid date format. Please enter date of birth (YYYY-MM-DD):");
        }

        UserManager.RegisterUser(username, password, dateOfBirth);
    }

    private bool IsAllLetters(string input)
    {
        return input.All(char.IsLetter);
    }

    private bool IsAllDigits(string input)
    {
        return input.All(char.IsDigit);
    }
}

class Login
{
    public string DoLogin()
    {
        Console.WriteLine("\n--- Login ---");
        Console.WriteLine("Input username:");
        string username = Console.ReadLine()!;
        Console.WriteLine("Input password:");
        string password = Console.ReadLine()!;

        if (UserManager.ValidateUser(username, password))
        {
            Console.WriteLine($"Login successful as {username}.");
            return username;
        }
        else
        {
            Console.WriteLine("Invalid username or password.");
            return null;
        }
    }
}

class QuizManager
{
    private const string questionsFile = "questions.json";
    private const string quizResultsFile = "quizResults.json";

    public void StartQuiz(string topic, string username)
    {
        Console.WriteLine($"\nStarting {topic} quiz for user {username}...");

        List<Question> questions = QuizPlace.GetQuestionsByTopic(topic);

        if (questions.Count == 0)
        {
            Console.WriteLine($"No questions available for {topic} quiz.");
            return;
        }

        int totalQuestions = 20;
        List<Question> selectedQuestions = questions.Take(totalQuestions).ToList();

        StartSelectedQuiz(selectedQuestions, username, topic);
    }

    public void StartMixedQuiz(string username)
    {
        Console.WriteLine($"\nStarting mixed quiz for user {username}...");

        List<Question> allQuestions = QuizPlace.GetAllQuestions();

        if (allQuestions == null || allQuestions.Count == 0)
        {
            Console.WriteLine("No questions available for mixed quiz.");
            return;
        }

        int totalQuestions = 20;
        List<Question> selectedQuestions = allQuestions.OrderBy(q => Guid.NewGuid()).Take(totalQuestions).ToList();

        StartSelectedQuiz(selectedQuestions, username, "Mixed Quiz");
    }

    private void StartSelectedQuiz(List<Question> selectedQuestions, string username, string topic)
    {
        int score = 0;
        List<bool> questionResults = new List<bool>();

        foreach (var question in selectedQuestions)
        {
            Console.WriteLine($"\nQuestion: {question.Text}");
            Console.WriteLine("Options:");
            for (int i = 0; i < question.Answers.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {question.Answers[i]}");
            }

            Console.Write("Your answer(s) (comma-separated for multiple answers): ");
            string userAnswerInput = Console.ReadLine()!;
            string[] userAnswers = userAnswerInput.Split(',');

            bool isCorrect = CheckAnswers(question, userAnswers);
            questionResults.Add(isCorrect);

            if (isCorrect)
            {
                Console.WriteLine("Correct!");
                score++;
            }
            else
            {
                Console.WriteLine($"Incorrect! Correct answer(s): {string.Join(", ", question.Correct.Select(i => question.Answers[i - 1]))}");
            }
        }

        Console.WriteLine($"\nQuiz completed! You scored {score} out of {selectedQuestions.Count}.");

        QuizPlace.SaveQuizResult(username, topic, score, questionResults);
    }

    public void ViewPastResults(string username)
    {
        Console.WriteLine($"\n--- Past Quiz Results for user {username} ---");

        List<QuizResult> userResults = QuizPlace.GetQuizResultsByUsername(username);

        if (userResults.Count == 0)
        {
            Console.WriteLine("No past quiz results found.");
        }
        else
        {
            foreach (var result in userResults)
            {
                Console.WriteLine($"Quiz: {result.Topic}, Score: {result.Score}, Date: {result.Date}");
            }
        }
    }

    public void ViewTopScores()
    {
        Console.WriteLine("\n--- Top 20 Quiz Scores ---");

        List<QuizResult> topScores = QuizPlace.GetTopQuizScores(20);

        if (topScores.Count == 0)
        {
            Console.WriteLine("No top scores found.");
        }
        else
        {
            foreach (var score in topScores)
            {
                Console.WriteLine($"User: {score.Username}, Topic: {score.Topic}, Score: {score.Score}");
            }
        }
    }

    private bool CheckAnswers(Question question, string[] userAnswers)
    {
        List<int> correctAnswers = question.Correct;

        if (userAnswers.Length != correctAnswers.Count)
        {
            return false;
        }

        foreach (var answer in userAnswers)
        {
            if (!int.TryParse(answer.Trim(), out int answerIndex) || !correctAnswers.Contains(answerIndex))
            {
                return false;
            }
        }

        return true;
    }
}

class QuizPlace
{
    private const string questionsFile = "questions.json";
    private const string quizResultsFile = "quizResults.json";

    private static List<Question> questions = LoadQuestionsFromFile();
    private static List<QuizResult> quizResults = LoadQuizResultsFromFile();

    private static List<Question> LoadQuestionsFromFile()
    {
        if (File.Exists(questionsFile))
        {
            string json = File.ReadAllText(questionsFile);
            if (!string.IsNullOrWhiteSpace(json))
            {
                List<QuizTopic> quizTopics = JsonSerializer.Deserialize<List<QuizTopic>>(json)!;
                if (quizTopics != null)
                {
                    List<Question> allQuestions = new List<Question>();
                    foreach (var topic in quizTopics)
                    {
                        foreach (var question in topic.Questions)
                        {
                            question.Topic = topic.Title;
                            allQuestions.Add(question);
                        }
                    }
                    return allQuestions;
                }
            }
        }
        return new List<Question>();
    }

    private static List<QuizResult> LoadQuizResultsFromFile()
    {
        if (File.Exists(quizResultsFile))
        {
            string json = File.ReadAllText(quizResultsFile);
            if (!string.IsNullOrWhiteSpace(json))
            {
                return JsonSerializer.Deserialize<List<QuizResult>>(json) ?? new List<QuizResult>();
            }
        }
        return new List<QuizResult>();
    }

    public static List<Question> GetQuestionsByTopic(string topic)
    {
        return questions.Where(q => q.Topic.Equals(topic, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public static List<Question> GetAllQuestions()
    {
        return questions;
    }

    public static List<QuizResult> GetQuizResultsByUsername(string username)
    {
        return quizResults.Where(qr => qr.Username!.Equals(username, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public static List<QuizResult> GetTopQuizScores(int top)
    {
        return quizResults.OrderByDescending(qr => qr.Score).Take(top).ToList();
    }

    public static void SaveQuizResult(string username, string topic, int score, List<bool> questionResults)
    {
        var newResult = new QuizResult
        {
            Username = username,
            Topic = topic,
            Score = score,
            QuestionResults = questionResults,
            Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        quizResults.Add(newResult);
        SaveQuizResultsToFile();
    }

    private static void SaveQuizResultsToFile()
    {
        string json = JsonSerializer.Serialize(quizResults, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(quizResultsFile, json);
    }
}

class QuizTopic
{
    public string Title { get; set; }
    public List<Question> Questions { get; set; }
}

class Question
{
    public string? Text { get; set; }
    public List<string> Answers { get; set; }
    public List<int> Correct { get; set; }
    public string? Topic { get; set; } 
}

class QuizResult
{
    public string? Username { get; set; }
    public string? Topic { get; set; }
    public int Score { get; set; }
    public string? Date { get; set; }
    public List<bool> QuestionResults { get; set; }
}

class UserManager
{
    private const string  usersFile = "users.txt";

    private static List<User> users = LoadUsersFromFile();

    public static void RegisterUser(string username, string password, DateTime dateOfBirth)
    {
        if (users.Any(u => u.Username!.Equals(username, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("Username already exists. Please choose another.");
            return;
        }

        var newUser = new User { Username = username, Password = password, DateOfBirth = dateOfBirth };
        users.Add(newUser);
        SaveUsersToFile();
        Console.WriteLine("Registration successful.");
    }

    public static bool ValidateUser(string username, string password)
    {
        return users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Password == password);
    }

    public static void UpdateUserSettings(string username)
    {
        var user = users.FirstOrDefault(u => u.Username!.Equals(username, StringComparison.OrdinalIgnoreCase));
        if (user == null)
        {
            Console.WriteLine("User not found.");
            return;
        }

        Console.WriteLine("\n--- Change Settings ---");
        Console.WriteLine("1. Change Password");
        Console.WriteLine("2. Change Date of Birth");
        Console.WriteLine("3. Back to Main Menu");

        int choice = GetChoice();

        switch (choice)
        {
            case 1:
                Console.WriteLine("Enter new password:");
                string newPassword = Console.ReadLine()!;
                user.Password = newPassword!;
                SaveUsersToFile();
                Console.WriteLine("Password updated successfully.");
                break;
            case 2:
                Console.WriteLine("Enter new date of birth (YYYY-MM-DD):");
                if (DateTime.TryParse(Console.ReadLine(), out DateTime newDateOfBirth))
                {
                    user.DateOfBirth = newDateOfBirth;
                    SaveUsersToFile();
                    Console.WriteLine("Date of birth updated successfully.");
                }
                else
                {
                    Console.WriteLine("Invalid date format. Date of birth not updated.");
                }
                break;
            case 3:
                Console.WriteLine("Returning to Main Menu...");
                break;
            default:
                Console.WriteLine("Invalid choice.");
                break;
        }
    }

    private static List<User> LoadUsersFromFile()
    {
        List<User> loadedUsers = new List<User>();

        if (File.Exists(usersFile))
        {
            using (StreamReader reader = new StreamReader(usersFile))
            {
                string line;
                while ((line = reader.ReadLine()!) != null)
                {
                    string[] parts = line.Split('|');
                    if (parts.Length == 3)
                    {
                        if (DateTime.TryParse(parts[2], out DateTime dateOfBirth))
                        {
                            loadedUsers.Add(new User { Username = parts[0], Password = parts[1], DateOfBirth = dateOfBirth });
                        }
                    }
                }
            }
        }

        return loadedUsers;
    }

    private static void SaveUsersToFile()
    {
        using (StreamWriter writer = new StreamWriter(usersFile))
        {
            foreach (var user in users)
            {
                writer.WriteLine($"{user.Username}|{user.Password}|{user.DateOfBirth.ToString("yyyy-MM-dd")}");
            }
        }
    }

    private static int GetChoice()
    {
        int choice;
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out choice) && choice >= 1 && choice <= 3)
            {
                return choice;
            }
            else
            {
                Console.WriteLine("Please input a number from 1 to 3.");
            }
        }
    }
}

class User
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public DateTime DateOfBirth { get; set; } 
}