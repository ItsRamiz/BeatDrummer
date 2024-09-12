using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

[System.Serializable]
public class Note
{
    public int startFrame;
    public int endFrame;
    public int hitKey;
    public int generateNote;
    public Note(int startFrame,int hitKey) // 0 - Z || 1 - X || 2 - C
    {
        this.generateNote = startFrame - 60;
        this.startFrame = startFrame - 25;
        this.endFrame = startFrame + 25;
        this.hitKey = hitKey;
    }
}
public class NotesData
{
    public List<Note> notes { get; set; }
}

public class Gamemode : MonoBehaviour
{

    public GameTimer gameTimer;
    public MusicHandler handler;


    public bool gameTimerEnable = false;
    public bool playMusic = false;

    public int currentNote = 0;

    public Note[] notes = new Note[17];

    public int[] notesTrack = new int[4];

    public Material greenMaterial;
    public Material redMaterial;
    public Material yellowMaterial;
    public Material blueMaterial;

    public GameObject obj1;

    public GameObject obj2;

    public GameObject obj3;

    public GameObject redDrum;

    public GameObject blueDrum;

    public GameObject greenDrum;

    public GameObject yellowDrum;

    public Renderer renderer1;

    public Renderer renderer2;

    public Renderer renderer3;

    public Renderer redDrumRenderer;

    public Renderer blueDrumRenderer;

    public Renderer yellowDrumRenderer;

    public Renderer greenDrumRenderer;

    public Renderer flyingSphereRenderer;

    public int HitBoxes;

    public int MissBoxes;

    public TMPro.TextMeshProUGUI hitBoxesText;

    public TMPro.TextMeshProUGUI missBoxesText;

    public TextMeshPro Instruct;

    public Boolean missed;

    public GameObject flyingObject;

    public Vector3 startPosition;

    public List<Note> notesJSON;

    // Start is called before the first frame update
    void Start()
    {
        string json = File.ReadAllText("C:\\Users\\Ramiz\\wkspaces\\BeatDrummerGamee\\Assets\\customLevels.json");
        NotesData notesData = JsonConvert.DeserializeObject<NotesData>(json);

        notesJSON = notesData.notes;

        Debug.Log(notesJSON.Count);


        Instruct.color = Color.red;

        missed = true;

        currentNote = 0;

        greenMaterial = Resources.Load<Material>("Materials/greenMaterial");
        redMaterial = Resources.Load<Material>("Materials/redMaterial");
        yellowMaterial = Resources.Load<Material>("Materials/yellowMaterial");
        blueMaterial = Resources.Load<Material>("Materials/blueMaterial");

        obj1 = GameObject.FindGameObjectWithTag("leftBox");
        renderer1 = obj1.GetComponent<Renderer>();

        obj2 = GameObject.FindGameObjectWithTag("midBox");
        renderer2 = obj2.GetComponent<Renderer>();

        obj3 = GameObject.FindGameObjectWithTag("rightBox");
        renderer3 = obj3.GetComponent<Renderer>();

        redDrum = GameObject.FindGameObjectWithTag("redDrum");
        redDrumRenderer = redDrum.GetComponent<Renderer>();

        blueDrum = GameObject.FindGameObjectWithTag("blueDrum");
        blueDrumRenderer = blueDrum.GetComponent<Renderer>();

        greenDrum = GameObject.FindGameObjectWithTag("greenDrum");
        greenDrumRenderer = greenDrum.GetComponent<Renderer>();

        yellowDrum = GameObject.FindGameObjectWithTag("yellowDrum");
        yellowDrumRenderer = yellowDrum.GetComponent<Renderer>();

        flyingSphereRenderer = flyingObject.GetComponent<Renderer>();



        HitBoxes = 0;
        MissBoxes = 0;

        hitBoxesText.text = "Hits = " + 0;

        missBoxesText.text = "Misses = " + 0;

        startPosition = flyingObject.transform.position;

        //renderer.material = greenMaterial; CHANGE OBJECT MATERIAL

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) // Pressing P starts the game.
        {
            Instruct.color = Color.green;
            Instruct.text = "PLAYING";
            gameTimerEnable = true;
            playMusic = true;
            gameTimer.setEnable(true);
            handler.setEnable(true);
        }

        if (gameTimerEnable)
        {
            if (currentNote < notesJSON.Count)
            {
                for (int i = 0; i < notesJSON.Count; i++)
                {
                    if (gameTimer.currentFrame == notesJSON[i].generateNote)
                    {
                        Debug.Log("Must Generate Note");
                        if (notesJSON[currentNote].hitKey == 0)
                        {
                            StartCoroutine(CreateAndMoveFlyingObject(0));
                        }
                        if (notesJSON[currentNote].hitKey == 1)
                        {
                            StartCoroutine(CreateAndMoveFlyingObject(1));
                        }
                        if (notesJSON[currentNote].hitKey == 2)
                        {
                            StartCoroutine(CreateAndMoveFlyingObject(2));
                        }
                    }
                }
                if (gameTimer.currentFrame == notesJSON[currentNote].startFrame)
                {
                    missed = true;
                    if (notesJSON[currentNote].hitKey == 0)
                    {
                        renderer1.material = yellowMaterial;
                        renderer2.material = redMaterial;
                        renderer3.material = redMaterial;
                    }
                    if (notesJSON[currentNote].hitKey == 1)
                    {
                        renderer1.material = redMaterial;
                        renderer2.material = yellowMaterial;
                        renderer3.material = redMaterial;
                    }
                    if (notesJSON[currentNote].hitKey == 2)
                    {
                        renderer1.material = redMaterial;
                        renderer2.material = redMaterial;
                        renderer3.material = yellowMaterial;
                    }
                }
                if (gameTimer.currentFrame >= notesJSON[currentNote].startFrame && gameTimer.currentFrame <= notesJSON[currentNote].endFrame)
                {
                    if (Input.GetKeyDown(KeyCode.Z))
                    {
                        if (notesJSON[currentNote].hitKey == 0)
                        {
                            HitBoxes++;
                            renderer1.material = greenMaterial;
                            currentNote++; // Move to the next note
                            missed = false;
                            hitBoxesText.text = "Hits = " + HitBoxes;
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.X))
                    {
                        if (notesJSON[currentNote].hitKey == 1)
                        {
                            HitBoxes++;
                            renderer2.material = greenMaterial;
                            currentNote++; // Move to the next note
                            missed = false;
                            hitBoxesText.text = "Hits = " + HitBoxes;
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.C))
                    {
                        if (notesJSON[currentNote].hitKey == 2)
                        {
                            HitBoxes++;
                            renderer3.material = greenMaterial;
                            currentNote++; // Move to the next note
                            missed = false;
                            hitBoxesText.text = "Hits = " + HitBoxes;
                        }
                    }
                }
                if (gameTimer.currentFrame > notesJSON[currentNote].endFrame && currentNote < notesJSON.Count && missed == true)
                {
                    currentNote++;
                    MissBoxes++;
                    renderer1.material = redMaterial;
                    renderer2.material = redMaterial;
                    renderer3.material = redMaterial;
                    missBoxesText.text = "Misses = " + MissBoxes;
                }
            }
        }
    }
    IEnumerator CreateAndMoveFlyingObject(int identifier)
    {
        Vector3 startPosition = Vector3.zero;
        Vector3 offset = new Vector3(0, 1, 20); // The offset to add to the target object's position

        if (identifier == 0)
        {
            startPosition = GameObject.FindGameObjectWithTag("redDrum").transform.position + offset;
            flyingSphereRenderer.material = redMaterial;
        }
        else if (identifier == 1)
        {
            startPosition = GameObject.FindGameObjectWithTag("blueDrum").transform.position + offset;
            flyingSphereRenderer.material = blueMaterial;
        }
        else if (identifier == 2)
        {
            startPosition = GameObject.FindGameObjectWithTag("greenDrum").transform.position + offset;
            flyingSphereRenderer.material = greenMaterial;
        }
        else
        {
            startPosition = GameObject.FindGameObjectWithTag("yellowDrum").transform.position + offset;
            flyingSphereRenderer.material = yellowMaterial;
        }

        GameObject flyingObjectInstance = Instantiate(flyingObject);


        flyingObjectInstance.transform.position = startPosition;
        flyingObjectInstance.SetActive(true);

        float distanceToTravel = 20f;
        float speed = 25f;

        while (Vector3.Distance(startPosition, flyingObjectInstance.transform.position) < distanceToTravel)
        {
            flyingObjectInstance.transform.Translate(Vector3.back * speed * Time.deltaTime);
            yield return null; // Wait for the next frame
        }

        // Disable the object after it has traveled the specified distance
        flyingObjectInstance.SetActive(false);
        // Alternatively, you can destroy it
        // Destroy(flyingObjectInstance);
    }
}
