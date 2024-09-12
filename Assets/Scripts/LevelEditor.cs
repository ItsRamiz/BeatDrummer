using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using Unity.VRTemplate;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System;
using TreeEditor;

public class RandomStringGenerator
{
    private static System.Random random = new System.Random(); // Use the full namespace to avoid conflicts

    public static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            result.Append(chars[random.Next(chars.Length)]); // Use the static random instance
        }

        return result.ToString();
    }
}
[System.Serializable]
public class SerializableVector3
{
    public float x, y, z;

    public SerializableVector3(Vector3 vector)
    {
        this.x = vector.x;
        this.y = vector.y;
        this.z = vector.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(this.x, this.y, this.z);
    }
}

[System.Serializable]
public class Drum
{
    public int drumID;
    public SerializableVector3 drumPosition;
    public SerializableVector3 drumRotation;

    [JsonIgnore]
    public GameObject DrumBallPrefab;

    public Drum(int drumID, Vector3 drumPosition, GameObject drumBallPrefab, Vector3 drumRotation = default(Vector3))
    {
        this.drumID = drumID;
        this.drumPosition = new SerializableVector3(drumPosition);
        this.drumRotation = new SerializableVector3(drumRotation == default(Vector3) ? Vector3.zero : drumRotation);
        this.DrumBallPrefab = drumBallPrefab;
    }

    public GameObject GetDrumBall()
    {
        return this.DrumBallPrefab;
    }

    public Vector3 GetPosition()
    {
        return drumPosition.ToVector3();
    }

    public Vector3 GetRotation()
    {
        return drumRotation.ToVector3();
    }
}

[System.Serializable]
public class DrumNote
{
    public int Id;
    public int HitFrame;
    public int DrumId;

    [JsonIgnore]
    public GameObject Button;

    public DrumNote(int id, int hitFrame, int drumId, GameObject button)
    {
        this.Id = id;
        this.HitFrame = hitFrame;
        this.DrumId = drumId;
        this.Button = button;
    }

    public int GetFrame()
    {
        return this.HitFrame;
    }
}
public class CustomLevel
{
    public Stack<Drum> DrumsConfig;
    public SortedList<int, DrumNote> NotesArray;
    public string SongPath;

    public CustomLevel(Stack<Drum> drumsConfig, SortedList<int, DrumNote> notesArray, string songPath)
    {
        this.DrumsConfig = drumsConfig;
        this.NotesArray = notesArray;
        this.SongPath = songPath;
    }

    public string SerializeLevel()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    public void SaveLevel(string fileName)
    {
        UpdateDrumTransforms();

        string json = SerializeLevel();
        string directoryPath = "C:\\Users\\Ramiz\\VRSample\\Assets\\customLevelsDir";

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            Debug.Log("Created directory: " + directoryPath);
        }

        string filePath = Path.Combine(directoryPath, $"{RandomStringGenerator.GenerateRandomString(6)}.json");
        filePath = filePath.Replace("\\", "/");

        File.WriteAllText(filePath, json);
        Debug.Log("JSON saved to: " + filePath);
    }

    public static CustomLevel LoadLevel(string fileName)
    {
        string directoryPath = Path.Combine(Application.dataPath, "customLevels");
        string filePath = Path.Combine(directoryPath, $"{fileName}.json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<CustomLevel>(json);
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
            return null;
        }
    }

    private void UpdateDrumTransforms()
    {
        foreach (var drum in DrumsConfig)
        {
            if (drum.DrumBallPrefab != null)
            {
                drum.drumPosition = new SerializableVector3(drum.DrumBallPrefab.transform.position);
                drum.drumRotation = new SerializableVector3(drum.DrumBallPrefab.transform.rotation.eulerAngles);
            }
        }
    }
}

public class LevelEditor : MonoBehaviour
{
    public string levelsDirectoryPath = "C:\\Users\\Ramiz\\VRVersion\\Assets\\customAssets\\Levels";
    public string customLevelsDirectoryPath = "C:\\Users\\Ramiz\\VRSample\\Assets\\customLevelsDir";
    public GameObject chooseSongButtonPrefab;
    public GameObject DrumBallPrefab;
    public GameObject chooseSongPanel;
    public GameObject mainMenuPanel;
    public GameObject customChooseSongPanel;
    public SortedList<int, DrumNote> notesArray = new SortedList<int, DrumNote>();
    public int levelCount;
    public int customLevelCount;
    public VideoPlayer videoPlayer; // Reference to the VideoPlayer
    public VideoPlayer PreviewVideoPlayer;

    public string[] levelFiles;
    public string[] customLevelFiles;
    public Button addDrumButton;
    public Button removeDrumButton;
    public Button moveDrumsButton;
    public Button TestButton;
    public Button SaveLevel;
    public Button LoadLevel;

    public Button playGame;
    public Button playEditor;

    public bool moveDrumsBool;
    public int spawnedDrumsCount;
    public Vector3 spawnPositionn = new Vector3(-10.24f, 0.37f, -1.31f);

    public Stack<Drum> drums_Queue = new Stack<Drum>();


    public GameObject leftController;
    public GameObject rightController;
    public GameObject leftDrumStick;
    public GameObject rightDrumStick;

    public GameObject notesBoard;
    public GameObject notesBoardButtonPrefab;
    public GameObject editorOptionsMenu;

    public Button EnableEditorButton;
    public bool enabledEditor;

    public int currentSelectedSong;
    public string currentSelectedSongPath;

    public bool isPlaying;
    public int currentPlayingNote;
    public int currentGameplayFrame;
    public int score;

    public Button BackButton;

    public GameObject PreviewVideoMenu;

    public VideoTimeScrubControl videoTimeScrubControl;

    private GameObject currentlySelectedButton = null;

    public GameObject WholePreviewPlayer;

    public Button StopRecording;

    public GameObject WholeVideoPlayer;

    public GameObject WholeNotesBoard;

    public Button StopPlaying;


    public RectTransform sliderRectTransform;

    public GameObject noteCapsulePrefab;

    float coroutineCooldown = 1.0f;  // 1 second cooldown between coroutine calls
    float lastCoroutineTime = 0f;  // Time since last coroutine call

    public Vector3 newVidPos = Vector3.zero;
    public Quaternion newVidRot = Quaternion.identity;
    public Vector3 newVidScale = Vector3.one;

    public Vector3 oldVidPos = Vector3.zero;
    public Quaternion oldVidRot = Quaternion.identity;
    public Vector3 oldVidScale = Vector3.one;

    public class TempCustomLevel
    {
        public List<Drum> DrumsConfig { get; set; }  // Use List here to deserialize
        public SortedList<int, DrumNote> NotesArray { get; set; }
        public string SongPath { get; set; }
    }
    // Start is called before the first frame update

    public void ForwardPreview()
    {
        WholePreviewPlayer.transform.position = newVidPos;

        // Copy rotation
        WholePreviewPlayer.transform.rotation = newVidRot;

        WholePreviewPlayer.transform.localScale = newVidScale;
    }
    public void BackPreview()
    {
        WholePreviewPlayer.transform.position = oldVidPos;
        WholePreviewPlayer.transform.rotation = oldVidRot;
        WholePreviewPlayer.transform.localScale = oldVidScale;

    }


    void Start()
    {
        Application.targetFrameRate = 72;

        Vector3 offset = new Vector3(0, 1, 0);  // Adjust to 10-unit travel instead of 20

        newVidPos = WholeVideoPlayer.transform.position + offset;
        newVidRot = WholeVideoPlayer.transform.rotation;
        newVidScale = WholeVideoPlayer.transform.localScale;

        oldVidPos = WholePreviewPlayer.transform.position;
        oldVidRot = WholePreviewPlayer.transform.rotation;
        oldVidScale = WholePreviewPlayer.transform.localScale;

        score = 0;
        isPlaying = false;
        enabledEditor = false;
        moveDrumsBool = true;
        spawnedDrumsCount = 0;
        levelCount = CountFilesInDirectory(levelsDirectoryPath);
        levelFiles = GetFileNamesInDirectory(levelsDirectoryPath);

        customLevelCount = CountFilesInDirectoryJSON("C:\\Users\\Ramiz\\VRSample\\Assets\\customLevelsDir");
        customLevelFiles = GetFileNamesInDirectoryJSON("C:\\Users\\Ramiz\\VRSample\\Assets\\customLevelsDir");

        //Debug.Log("CustonFiles == " + customLevelFiles.Length);

        //Debug.Log("CustomLevelCount = " + customLevelCount);

        LoadCustomButtons();






        addDrumButton.onClick.AddListener(addDrumToWorld);
        removeDrumButton.onClick.AddListener(removeDrumFromWorld);
        moveDrumsButton.onClick.AddListener(moveDrumsEnable);
        TestButton.onClick.AddListener(Test);
        SaveLevel.onClick.AddListener(SaveCustomLevel);
        LoadLevel.onClick.AddListener(LoadCustomLevel);
        EnableEditorButton.onClick.AddListener(EnableEditor);

        playGame.onClick.AddListener(PlayGameFunction);
        playEditor.onClick.AddListener(PlayEditorFunction);
        StopPlaying.onClick.AddListener(StopPlayingF);

        BackButton.onClick.AddListener(MenuBack);

        StopRecording.onClick.AddListener(StopRecordingF);

        // Subscribe to the drum touched event
        DrumTriggerInteraction.OnDrumTouched += HandleDrumTouched;
        //Debug.Log("Subscribing to OnDrumTouched event");

        EnableEditorButton.GetComponentInChildren<TextMeshProUGUI>().text = "Editor Disabled";
        EnableEditorButton.image.color = Color.red; // Set button color to red

        editorOptionsMenu.SetActive(false);

        videoTimeScrubControl = FindObjectOfType<VideoTimeScrubControl>();

        EnableEditorButton.gameObject.SetActive(false);

        BackButton.gameObject.SetActive(false);

        chooseSongPanel.gameObject.SetActive(false);

        customChooseSongPanel.gameObject.SetActive(false);

        WholeNotesBoard.SetActive(false);

        editorOptionsMenu.SetActive(false);

        PreviewVideoMenu.SetActive(false);

        LoadPreviewButtons();

        WholePreviewPlayer.gameObject.SetActive(false);

        StopRecording.gameObject.SetActive(false);

        WholeVideoPlayer.gameObject.SetActive(false);

        WholeNotesBoard.SetActive(false);

        StopPlaying.gameObject.SetActive(false);


    }
    private float deltaTime = 0.0f;
    private HashSet<int> processedFrames = new HashSet<int>();

    // Update is called once per frame
    void Update()
    {
        if (isPlaying)
        {
            currentGameplayFrame = (int)videoTimeScrubControl.GetCurrentFrame();

            if (!processedFrames.Contains(currentGameplayFrame))
            {
                if (notesArray.ContainsKey(currentGameplayFrame + 60))
                {
                    processedFrames.Add(currentGameplayFrame);
                    int drumID = notesArray[currentGameplayFrame + 60].DrumId;
                    StartCoroutine(CreateAndMoveFlyingObject(drumID, 72));

                    // Mark the currentGameplayFrame as processed
                }
            }
        }
    }

    void StopPlayingF()
    {

        BackPreview();

        StopPlaying.gameObject.SetActive(false);

        WholeVideoPlayer.SetActive(false);

        WholePreviewPlayer.SetActive(false);

        chooseSongPanel.SetActive(true);

        BackButton.gameObject.SetActive(true);

        foreach (var drum in drums_Queue)
        {
            Destroy(drum.DrumBallPrefab);
        }
        drums_Queue.Clear();

        spawnedDrumsCount = 0;


    }
    void CreateButtonsForVideoFrames()
    {
        VideoTimeScrubControl videoTimeScrubControl = FindObjectOfType<VideoTimeScrubControl>();

        long totalFrames = (long)videoTimeScrubControl.GetTotalFrames();

        //Debug.Log("Total Frames = " + totalFrames);

        // Padding between buttons in terms of X position (adjust to fit your needs)
        float padding = 10f;

        // Set button size (larger width and height)
        Vector2 buttonSize = new Vector2(150, 75);  // Larger size for more visibility

        // Loop through each 900-frame segment and instantiate a button
        for (long frame = 900; frame < totalFrames; frame += 900)
        {

            // Calculate the position on the slider as a percentage
            float sliderPosition = (float)frame / totalFrames;

            // Instantiate a button
            GameObject newButton = Instantiate(chooseSongButtonPrefab, sliderRectTransform);

            // Ensure the button is properly anchored to the slider
            RectTransform buttonRect = newButton.GetComponent<RectTransform>();

            // Reset the scale (ensure it's (1, 1, 1) in local space)
            buttonRect.localScale = Vector3.one;

            // Set the button's size to a larger value
            buttonRect.sizeDelta = buttonSize;

            // Calculate the X position based on slider percentage and add padding
            float xPos = sliderRectTransform.rect.width * sliderPosition + padding * (frame / 900);

            // Set button's anchored position (adjust Y for above the slider)
            buttonRect.anchoredPosition = new Vector2(xPos, 50); // Adjust Y for positioning the button above the slider
            buttonRect.anchorMin = new Vector2(0, 0.5f); // Anchor to the center vertically
            buttonRect.anchorMax = new Vector2(0, 0.5f); // Set max anchor to center vertically
            buttonRect.pivot = new Vector2(0.5f, 0.5f);  // Set pivot to the center
        }
    }

    void PlayGameFunction()
    {
        ForwardPreview();

        BackButton.gameObject.SetActive(true);

        chooseSongPanel.gameObject.SetActive(true);

        Vector3 mainMenuPosition = mainMenuPanel.transform.position;
        Quaternion rotation = mainMenuPanel.transform.rotation;

        chooseSongPanel.transform.position = mainMenuPosition;
        chooseSongPanel.transform.rotation = rotation;

        mainMenuPanel.SetActive(false);
        EnableEditorButton.gameObject.SetActive(false);

        isPlaying = true;

    }

    void PlayEditorFunction()
    {

        BackButton.gameObject.SetActive(true);

        editorOptionsMenu.SetActive(true);

        Vector3 mainMenuPosition = mainMenuPanel.transform.position;

        Quaternion rotation = mainMenuPanel.transform.rotation;

        editorOptionsMenu.transform.rotation = rotation;
        editorOptionsMenu.transform.position = mainMenuPosition;

        mainMenuPanel.SetActive(false);

        PreviewVideoMenu.SetActive(true);


        WholePreviewPlayer.gameObject.SetActive(true);

        foreach (var note in notesArray.Values)
        {
            if (note.Button != null)
            {
                // Destroy the associated Button GameObject
                Destroy(note.Button);
            }
        }

        notesArray.Clear();
        foreach (var drum in drums_Queue)
        {
            Destroy(drum.DrumBallPrefab);
        }
        drums_Queue.Clear();

        spawnedDrumsCount = 0;

    }

    void MenuBack()
    {
        if(chooseSongPanel.activeSelf == true)
        {
            chooseSongPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
        if (editorOptionsMenu.activeSelf == true)
        {
            editorOptionsMenu.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
        BackButton.gameObject.SetActive(false);


        WholePreviewPlayer.gameObject.SetActive(false);

        PreviewVideoMenu.SetActive(false);

        foreach (var note in notesArray.Values)
        {
            if (note.Button != null)
            {
                // Destroy the associated Button GameObject
                Destroy(note.Button);
            }
        }

        // Clear the notesArray to remove all data
        notesArray.Clear();

        notesArray.Clear();
        foreach (var drum in drums_Queue)
        {
            Destroy(drum.DrumBallPrefab);
        }
        drums_Queue.Clear();

        spawnedDrumsCount = 0;

    }
    void StopRecordingF()
    {

        moveDrumsEnable();
        BackPreview();
        PreviewVideoPlayer.Pause();

    }


    void EnableEditor()
    {
        if(enabledEditor == false)
        {
            notesBoard.SetActive(false);
            editorOptionsMenu.SetActive(false);

            EnableEditorButton.GetComponentInChildren<TextMeshProUGUI>().text = "Editor Disabled";
            EnableEditorButton.image.color = Color.red; // Set button color to red

            enabledEditor = true; 
        }
        else
        {
            notesBoard.SetActive(true);
            editorOptionsMenu.SetActive(true);

            EnableEditorButton.GetComponentInChildren<TextMeshProUGUI>().text = "Editor Enabled";
            EnableEditorButton.image.color = Color.green; // Set button color to green

            enabledEditor = false; 
        }

    }


    void Test()
    {
        SimulateTouch();
        // Disable controllers
        //leftController.SetActive(false);
        //rightController.SetActive(false);

        // Enable spheres
        //leftDrumStick.SetActive(true);
        //rightDrumStick.SetActive(true);

    }

    public void SaveCustomLevel()
    {
        // Create a new CustomLevel object using the current drums and notes
        CustomLevel newLevel = new CustomLevel(drums_Queue, notesArray, currentSelectedSongPath);

        // Save the level directly using the SaveLevel method in the CustomLevel class
        newLevel.SaveLevel("Some Song Name");

        Debug.Log("Level saved successfully!");
    }
    public void LoadCustomLevel()
    {


    }

    public void AddButton(DrumNote newNote, string buttonText, int add = 1)
    {

        // Instantiate the button prefab
        GameObject newButton = Instantiate(notesBoardButtonPrefab, notesBoard.transform);
        newNote.Button = newButton;
        // Set the button text using TextMeshProUGUI
        TextMeshProUGUI buttonTextComponent = newButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonTextComponent != null)
        {
            buttonTextComponent.text = buttonText;
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found!");
        }
        if (add == 1)
        {
            // Create the DrumNote and add it to the sorted list
            if (notesArray.ContainsKey(newNote.GetFrame()))
            {
                notesArray.Add(newNote.GetFrame() + 1, newNote);
            }
            else
            {
                notesArray.Add(newNote.GetFrame(), newNote);
            }
        }

        // Optionally, you can add a listener to the button's onClick event
        Button buttonComponent = newButton.GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.onClick.AddListener(() => removeNote(newNote));
        }
    }
    void removeNote(DrumNote note)
    {
        if (note.Button != null)
        {
            notesArray.Remove(note.HitFrame); // Remove the DrumNote from the sorted list
            Destroy(note.Button); // Destroy the button associated with the DrumNote
            Debug.Log("Button and DrumNote removed: " + note.Id);
        }
        else
        {
            Debug.LogError("Button reference is null for DrumNote: " + note.Id);
        }
    }

    void SimulateTouch()
    {
        foreach (Drum drum in drums_Queue)
        {
            HandleDrumTouched(drum.GetDrumBall());
        }
    }

    int CountFilesInDirectory(string path)
    {
        if (Directory.Exists(levelsDirectoryPath))
        {
            string[] files = Directory.GetFiles(path, "*.mp4");
            return files.Length;
        }
        else
        {
            Debug.LogError("Levels directory does not exist, Please Edit Path");
            return 0;
        }
    }
    int CountFilesInDirectoryJSON(string path)
    {
        if (Directory.Exists(levelsDirectoryPath))
        {
            string[] files = Directory.GetFiles(path, "*.json");
            return files.Length;
        }
        else
        {
            Debug.LogError("Levels directory does not exist, Please Edit Path");
            return 0;
        }
    }
    string[] GetFileNamesInDirectory(string path)
    {
        if (Directory.Exists(levelsDirectoryPath))
        {
            string[] files = Directory.GetFiles(path, "*.mp4");
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileName(files[i]);
            }
            return files;
        }
        else
        {
            Debug.LogError("Levels directory does not exist, Please Edit Path");
            return new string[0];
        }
    }
    string[] GetFileNamesInDirectoryJSON(string path)
    {
        if (Directory.Exists(levelsDirectoryPath))
        {
            string[] files = Directory.GetFiles(path, "*.json");
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileName(files[i]);
            }
            return files;
        }
        else
        {
            Debug.LogError("Levels directory does not exist, Please Edit Path");
            return new string[0];
        }
    }

    void LoadButtons()
    {
        for (int i = 0; i < levelCount; i++)
        {
            //Debug.Log("Created Button");
            GameObject newButton = Instantiate(chooseSongButtonPrefab, chooseSongPanel.transform);
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = "Level " + (i + 1);

            // Access the Button component explicitly from UnityEngine.UI
            UnityEngine.UI.Button buttonComponent = newButton.GetComponent<UnityEngine.UI.Button>();

            int index = i; // Capture the current index for the lambda expression

            // Assuming you have a function called OnButtonClick to handle the button click event
            buttonComponent.onClick.AddListener(() => OnButtonClick(index + 1));
        }
    }
    void LoadCustomButtons()
    {
        for (int i = 0; i < customLevelCount; i++)
        {
            //Debug.Log("Created Button");
            GameObject newButton = Instantiate(chooseSongButtonPrefab, chooseSongPanel.transform);
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = "Custom Level " + (i + 1);

            // Access the Button component explicitly from UnityEngine.UI
            UnityEngine.UI.Button buttonComponent = newButton.GetComponent<UnityEngine.UI.Button>();

            int index = i; // Capture the current index for the lambda expression

            // Assuming you have a function called OnButtonClick to handle the button click event
            buttonComponent.onClick.AddListener(() => CustomOnButtonClick(index + 1));
        }

    }

    void LoadPreviewButtons()
    {
        string[] videoFiles = Directory.GetFiles(levelsDirectoryPath, "*.mp4");

        foreach (string videoPath in videoFiles)
        {
            // Create a button for each video
            GameObject newButton = Instantiate(chooseSongButtonPrefab, PreviewVideoMenu.transform);

            // Set the button text to the name of the video (without path or extension)
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = Path.GetFileNameWithoutExtension(videoPath);

            // Add a listener to the button to handle clicks and pass the current button as a parameter
            newButton.GetComponent<Button>().onClick.AddListener(() => OnVideoButtonClick(videoPath, newButton));
        }
    }

    void OnVideoButtonClick(string videoPath, GameObject clickedButton)
    {
        // Set the video player's URL to the selected video
        PreviewVideoPlayer.url = videoPath;
        PreviewVideoPlayer.Pause();

        PreviewVideoPlayer.url = videoPath;
        PreviewVideoPlayer.Play();

        // If there's a previously selected button, reset its color
        if (currentlySelectedButton != null)
        {
            currentlySelectedButton.GetComponent<Image>().color = Color.white;  // Reset to default color (white)
        }

        // Set the clicked button as the currently selected one
        currentlySelectedButton = clickedButton;

        // Change the clicked button's background color to green
        clickedButton.GetComponent<Image>().color = Color.green;
    }
    void OnButtonClick(int levelIndex)
    {
        WholeVideoPlayer.SetActive(false);
        WholePreviewPlayer.SetActive(true);
        BackButton.gameObject.SetActive(false);
        chooseSongPanel.SetActive(false);

        StopPlaying.gameObject.SetActive(true);

        Debug.Log("Button " + levelIndex + " clicked!");

        string videoPath = Path.Combine(levelsDirectoryPath, levelFiles[levelIndex - 1]);

        if (File.Exists(videoPath))
        {
            PreviewVideoPlayer.url = videoPath;
            PreviewVideoPlayer.Play();

            currentSelectedSong = levelIndex;
            currentSelectedSongPath = videoPath;
        }
        else
        {
            Debug.LogError("Video file not found: " + videoPath);
        }
    }
    void CustomOnButtonClick(int levelIndex)
    {
        ForwardPreview();

        WholeVideoPlayer.SetActive(false);
        WholePreviewPlayer.SetActive(true);

        BackButton.gameObject.SetActive(false);
        chooseSongPanel.SetActive(false);

        StopPlaying.gameObject.SetActive(true);


        while (spawnedDrumsCount > 0)
        {
            GameObject lastDrum = drums_Queue.Pop().GetDrumBall();
            Destroy(lastDrum);
            spawnedDrumsCount--;
        }
        foreach (var note in notesArray.Values)
        {
            if (note.Button != null)
            {
                GameObject.Destroy(note.Button);
            }
        }

        notesArray.Clear();

        string customLevelsDirectoryPath = "C:\\Users\\Ramiz\\VRSample\\Assets\\customLevelsDir";
        string jsonFilePath = Path.Combine(customLevelsDirectoryPath, customLevelFiles[levelIndex - 1]);
        string jsonContent = File.ReadAllText(jsonFilePath);

        var tempData = JsonConvert.DeserializeObject<TempCustomLevel>(jsonContent);
        Stack<Drum> drumStack = new Stack<Drum>(tempData.DrumsConfig);
        CustomLevel customLevel = new CustomLevel(drumStack, tempData.NotesArray, tempData.SongPath);

        Stack<Drum> drumsConfig = customLevel.DrumsConfig;

        notesArray = customLevel.NotesArray;
        string songPath = customLevel.SongPath;

        foreach (var drum in drumsConfig)
        {
            Vector3 spawnPosition = drum.GetPosition();
            Quaternion spawnRotation = Quaternion.Euler(drum.GetRotation());
            addDrumToWorld(spawnPosition, spawnRotation);
        }

        foreach (var noteEntry in notesArray)
        {
            DrumNote note = noteEntry.Value;
            string buttonText = $"Note {note.GetFrame()}";
            AddButton(note, buttonText, 0);
        }

        // Set video URL and prepare video
        PreviewVideoPlayer.url = songPath;
        PreviewVideoPlayer.prepareCompleted += OnVideoPrepared;  // Subscribe to the prepareCompleted event
        PreviewVideoPlayer.Prepare();  // Start preparing the video

        isPlaying = true;
        currentPlayingNote = 0;
        currentGameplayFrame = 0;

        foreach (var drum in drums_Queue)
        {
            Renderer drumRenderer = drum.DrumBallPrefab.GetComponent<Renderer>();
            if (drumRenderer != null)
            {
                drumRenderer.material.SetColor("_BaseColor", Color.red);  // Change the drum's color
            }
        }

        foreach (Drum drumm in drums_Queue)
        {
            GameObject drum = drumm.GetDrumBall();
            Rigidbody rb = drum.GetComponent<Rigidbody>();
            rb.isKinematic = true;

            Collider collider = drum.GetComponent<Collider>();
            collider.isTrigger = true;

            XRGrabInteractable grabInteractable = drum.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.enabled = false;
            }
        }
    }

    // This method will be called when the video is ready
    void OnVideoPrepared(VideoPlayer source)
    {
        PreviewVideoPlayer.Play();  // Now that the video is prepared, play it
        CreateButtonsForVideoFrames();  // Create buttons for the frames after the video is prepared
    }

    void addDrumToWorld()
    {

        Vector3 newSpawnPoint = spawnPositionn;

        newSpawnPoint.z += spawnedDrumsCount * 0.5f;

        spawnedDrumsCount++;

        GameObject newBall = Instantiate(DrumBallPrefab, newSpawnPoint, Quaternion.identity);

        newBall.transform.position = newSpawnPoint;

        Drum newDrum = new Drum(spawnedDrumsCount, newSpawnPoint, newBall);

        drums_Queue.Push(newDrum);

    }

    void addDrumToWorld(Vector3 spawnPoint, Quaternion rotation)
    {
        Vector3 newSpawnPoint = spawnPoint;

        spawnedDrumsCount++;

        GameObject newBall = Instantiate(DrumBallPrefab, newSpawnPoint, rotation);

        newBall.transform.position = newSpawnPoint;

        Drum newDrum = new Drum(spawnedDrumsCount, newSpawnPoint, newBall);

        drums_Queue.Push(newDrum);

    }
    void removeDrumFromWorld()
    {
        if (spawnedDrumsCount > 0)
        {
            GameObject lastDrum = drums_Queue.Pop().GetDrumBall();
            Destroy(lastDrum);
            spawnedDrumsCount--;
        }
    }
    void moveDrumsEnable()
    {
        ForwardPreview();

        foreach (var note in notesArray.Values)
        {
            if (note.Button != null)
            {
                // Destroy the associated Button GameObject
                Destroy(note.Button);
            }
        }

        // Clear the notesArray to remove all data
        notesArray.Clear();

        if (moveDrumsBool)
        {
            WholeNotesBoard.SetActive(true);
            editorOptionsMenu.SetActive(false);
            WholePreviewPlayer.SetActive(false);
            PreviewVideoMenu.SetActive(false);
            StopRecording.gameObject.SetActive(true);
            BackButton.gameObject.SetActive(false);
            moveDrumsBool = false;
            WholeVideoPlayer.SetActive(false);
            WholePreviewPlayer.SetActive(true);
            foreach (Drum drumm in drums_Queue)
            {
                GameObject drum = drumm.GetDrumBall();
                Rigidbody rb = drum.GetComponent<Rigidbody>();
                rb.isKinematic = true;

                Collider collider = drum.GetComponent<Collider>();
                collider.isTrigger = true;

                XRGrabInteractable grabInteractable = drum.GetComponent<XRGrabInteractable>();
                if (grabInteractable != null)
                {
                    grabInteractable.enabled = false;
                }
            }


        }
        else
        {
            WholeNotesBoard.SetActive(false);
            WholeVideoPlayer.SetActive(false);
            editorOptionsMenu.SetActive(true);
            WholePreviewPlayer.SetActive(true);
            PreviewVideoMenu.SetActive(true);
            StopRecording.gameObject.SetActive(false);
            BackButton.gameObject.SetActive(true);
            moveDrumsBool = true;
            foreach (Drum drumm in drums_Queue)
            {
                GameObject drum = drumm.GetDrumBall();
                Rigidbody rb = drum.GetComponent<Rigidbody>();
                rb.isKinematic = false;

                Collider collider = drum.GetComponent<Collider>();
                collider.isTrigger = false;

                XRGrabInteractable grabInteractable = drum.GetComponent<XRGrabInteractable>();
                if (grabInteractable != null)
                {
                    grabInteractable.enabled = true;
                }
            }
        }
    }
    void ChangeButtonColor(Button button, Color newColor)
    {
        ColorBlock cb = button.colors;
        cb.normalColor = newColor;
        button.colors = cb;
    }

    void SetButtonText(Button button, string newText)
    {
        Text textComponent = button.GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            textComponent.text = newText;
        }
        else
        {
            TextMeshProUGUI tmpTextComponent = button.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpTextComponent != null)
            {
                tmpTextComponent.text = newText;
            }
            else
            {
                Debug.LogWarning("No Text or TextMeshProUGUI component found in button's children.");
            }
        }
    }
    void HandleDrumTouched(GameObject touchedDrum)
    {
        foreach (Drum drum in drums_Queue)
        {
            if (drum.GetDrumBall() == touchedDrum)
            {
                //Debug.Log("Touched drum ID: " + drum.drumID);
                // Perform additional actions with the touched drum
                YourFunction(drum);
                break;
            }
        }
    }

    void YourFunction(Drum drum)
    {
        VideoTimeScrubControl videoTimeScrubControl = FindObjectOfType<VideoTimeScrubControl>();
        int currentFrame = (int)videoTimeScrubControl.GetCurrentFrame();

        //Debug.Log("Function called for drum ID: " + drum.drumID);

        DrumNote newNote = new DrumNote(1,currentFrame, drum.drumID, null);

        AddButton(newNote,"Frame = " + currentFrame + " ID = " + drum.drumID);

        foreach (var drumm in drums_Queue)
        {
            Renderer drumRenderer = drumm.DrumBallPrefab.GetComponent<Renderer>();
            if (drumRenderer != null)
            {
                Color currentColor = drumRenderer.material.GetColor("_BaseColor");
                if (currentColor.Equals(Color.yellow))
                {
                    drumRenderer.material.SetColor("_BaseColor", Color.green);
                    score++;
                    Debug.Log("Score = " + score);
                }
            }
        }

    }
    IEnumerator CreateAndMoveFlyingObject(int identifier, int targetFrame)
    {
        // New slower speed (adjustable)
        float speed = 10f;  // Move slower at 10 units per second
        float travelDistance = 11f;  // Travel only 10 units
        int framesPerSecond = 72;  // Your game's FPS cap

        // Calculate how long it will take to travel 10 units at 10 units per second
        float travelTimeInSeconds = travelDistance / speed;
        int framesToTravel = Mathf.CeilToInt(travelTimeInSeconds * framesPerSecond);  // Frames needed to travel 10 units

        // Determine the frame when the note should be spawned
        int spawnFrame = targetFrame - framesToTravel;

        // Check if we are at or past the spawn frame
        if (currentGameplayFrame >= spawnFrame)
        {
            Vector3 offset = new Vector3(-10, 0, 0);  // Adjust to 10-unit travel instead of 20
            Vector3 drumPosition = GetDrumByID(identifier).GetPosition();
            Vector3 startPosition = drumPosition + offset;

            Debug.Log($"drum ID: {identifier}, current frame: {currentGameplayFrame}");

            // Instantiate and move the note towards the drum
            GameObject flyingObjectInstance = Instantiate(noteCapsulePrefab);
            flyingObjectInstance.transform.position = startPosition;
            flyingObjectInstance.SetActive(true);

            Vector3 destination = drumPosition;  // The target destination is the drum's position

            // Move the object towards the drum
            while (Vector3.Distance(flyingObjectInstance.transform.position, destination) > 0.1f)
            {
                // Calculate the direction from the current position to the destination
                Vector3 direction = (destination - flyingObjectInstance.transform.position).normalized;

                // Move the flying object in the direction of the destination
                flyingObjectInstance.transform.Translate(direction * speed * Time.deltaTime, Space.World);

                yield return null;  // Wait for the next frame
            }

            // Once the object has reached the destination, destroy it
            Destroy(flyingObjectInstance);
        }
    }
    public Drum GetDrumByID(int id)
    {
        foreach (Drum drum in drums_Queue)
        {
            if (drum.drumID == id)
            {
                return drum;
            }
        }
        return null;  // If no drum is found with the specified ID
    }

}
