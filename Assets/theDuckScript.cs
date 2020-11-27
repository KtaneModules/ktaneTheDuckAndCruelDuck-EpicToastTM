using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

public class theDuckScript : MonoBehaviour {

    public KMBombModule Module;
    public KMAudio Audio;
    public KMSelectable curtainSelectable;
    public KMSelectable[] duckParts, btnSelectables;
    public TextMesh[] btnText;
    public Material[] curtains;
    public Material open, solvedMat;
    public MeshRenderer surface;
    public MeshRenderer[] btnRenderers;

    private static int _moduleIdCounter = 1;
    private int _moduleId;
    private bool solved = false;
    
    private int curtainColor;
    private static readonly string[] colorNames = { "blue", "yellow", "green", "orange", "red" };
    private readonly string[] personalities = { "friendly", "impatient", "shy", "stubborn", "murderous" };
    private readonly string[] approaches = { "dive at the duck", "walk to the duck", "run to the duck", "sneak up on the duck", "swim to the duck", "fly to the duck", "approach the\nduck with caution" };
    private readonly string[] duckPartNames = { "belly", "afro", "beak", "left foot", "right foot", "eye", "tail" };
    
    private static readonly int[] lumpOfApproaches = { 1, 2, 3, 2, 5, 0, 3, 4, 6, 0, 4, 3, 6, 3, 0 };
    private int correctApproach;
    private int[] literallyABunchOfNumbers = { 0, 1, 2, 3, 4, 5, 6 };
    private int[] shownApproaches;
    private bool btnPressed = false;

    // Use this for initialization
    void Start () {
        _moduleId = _moduleIdCounter++;
        Module.OnActivate += Activate;

        Init();
    }
	
    void Activate()
    {
        for (int i = 0; i < 7; i++)
        {
            int j = i;
            duckParts[i].OnInteract += delegate ()
            {
                if (!solved)
                    Press(j);
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
                duckParts[j].AddInteractionPunch(2);

                if (solved)
                    Audio.PlaySoundAtTransform("quack", Module.transform);
                return false;
            };
        }
        
        for (int i = 0; i < 4; i++)
        {
            int j = i;
            btnSelectables[i].OnInteract += delegate ()
            {
                if (!solved)
                    BtnPress(j);
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
                btnSelectables[j].AddInteractionPunch();
                return false;
            };
        }

        curtainSelectable.OnInteract += delegate ()
        {
            if (!solved && curtainSelectable.enabled)
            {
                OpenCurtain();
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
                curtainSelectable.AddInteractionPunch();
            }

            return false;
        };
    }

    void Init()
    {
        btnPressed = false;
        curtainSelectable.enabled = true;
        for (int i = 0; i < 7; i++)
            duckParts[i].gameObject.SetActive(false);
        for (int i = 0; i < 4; i++)
        {
            btnSelectables[i].enabled = false;
            btnRenderers[i].gameObject.SetActive(false);
            btnText[i].text = "";
        }

        curtainColor = Random.Range(0, 5);
        literallyABunchOfNumbers = Enumerable.Range(0, 7).ToList().Shuffle().ToArray();

        shownApproaches = new int[4]{ literallyABunchOfNumbers[0], literallyABunchOfNumbers[1], literallyABunchOfNumbers[2], literallyABunchOfNumbers[3] };

        correctApproach = -1;
        while (correctApproach == -1)
        {
            for (int i = 0; i < 3; i++)
            {
                if (shownApproaches.Contains(lumpOfApproaches[i + 3 * curtainColor]))
                {
                    correctApproach = lumpOfApproaches[i + 3 * curtainColor];
                    break;
                }
            }
            if (correctApproach == -1)
            {
                literallyABunchOfNumbers = Enumerable.Range(0, 7).ToList().Shuffle().ToArray();
                shownApproaches = new int[4] { literallyABunchOfNumbers[0], literallyABunchOfNumbers[1], literallyABunchOfNumbers[2], literallyABunchOfNumbers[3] };
            }
        }

        Debug.LogFormat("[The Duck #{0}] The curtain is {1}.", _moduleId, colorNames[curtainColor]);
        Debug.LogFormat("[The Duck #{0}] Therefore, the duck is {1}.", _moduleId, personalities[curtainColor]);
        Debug.LogFormat("[The Duck #{0}] The possible approaches are {1}, {2}, {3}, and {4}.", _moduleId, approaches[shownApproaches[0]].Replace('\n', ' '), approaches[shownApproaches[1]].Replace('\n', ' '), approaches[shownApproaches[2]].Replace('\n', ' '), approaches[shownApproaches[3]].Replace('\n', ' '));
        Debug.LogFormat("[The Duck #{0}] The correct approach is to {1}.", _moduleId, approaches[correctApproach].Replace('\n', ' '));
        Debug.LogFormat("[The Duck #{0}] The correct part to click is the {1}.", _moduleId, duckPartNames[correctApproach]);

        surface.material = curtains[curtainColor];
    }

    void Press(int duckPart)
    {
        if (duckPart == correctApproach & btnPressed)
        {
            Module.HandlePass();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, Module.transform);
            Audio.PlaySoundAtTransform("solveSound", Module.transform);
            solved = true;
            surface.material = solvedMat;
            Debug.LogFormat("[The Duck #{0}] Module solved.", _moduleId);
        }

        else
        {
            Module.HandleStrike();
            Debug.LogFormat("[The Duck #{0}] You pressed an incorrect part of the duck! Strike.", _moduleId);
            Init();
        }
    }

    void BtnPress(int btnNumber)
    {
        btnPressed = true;
        if (shownApproaches[btnNumber] == correctApproach)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, Module.transform);
            Debug.LogFormat("[The Duck #{0}] Correct button pressed!", _moduleId);
            for (int i = 0; i < 7; i++)
                duckParts[i].gameObject.SetActive(true);
        }
        else
        {
            Module.HandleStrike();
            Debug.LogFormat("[The Duck #{0}] You approached incorrectly! Strike.", _moduleId);
            Init();
        }

        for (int i = 0; i < 4; i++)
        {
            btnRenderers[i].gameObject.SetActive(false);
            btnSelectables[i].enabled = false;
            btnText[i].text = "";
        }
    }

    void OpenCurtain()
    {
        curtainSelectable.enabled = false;
        surface.material = open;
        for (int i = 0; i < 4; i++)
        {
            btnRenderers[i].gameObject.SetActive(true);
            btnSelectables[i].enabled = true;
            btnText[i].text = approaches[shownApproaches[i]];
        }
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} curtain [Pulls back the curtain] | !{0} TL/TR/BL/BR [Presses the approach button in the specified positon (top-left, top-right, etc.)] | !{0} belly/beak/afro/tail/eye/left/right [Clicks the specified part of the duck]";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*curtain\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (!curtainSelectable.enabled)
            {
                yield return "sendtochaterror The curtain has already been pulled back!";
                yield break;
            }
            curtainSelectable.OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*tl\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*topleft\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*top-left\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (curtainSelectable.enabled)
            {
                yield return "sendtochaterror The curtain must be pulled back before an approach button can be pressed!";
                yield break;
            }
            else if (duckParts[0].gameObject.activeSelf)
            {
                yield return "sendtochaterror The duck has already been approached!";
                yield break;
            }
            btnSelectables[0].OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*bl\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*bottomleft\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*bottom-left\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (curtainSelectable.enabled)
            {
                yield return "sendtochaterror The curtain must be pulled back before an approach button can be pressed!";
                yield break;
            }
            else if (duckParts[0].gameObject.activeSelf)
            {
                yield return "sendtochaterror The duck has already been approached!";
                yield break;
            }
            btnSelectables[1].OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*tr\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*topright\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*top-right\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (curtainSelectable.enabled)
            {
                yield return "sendtochaterror The curtain must be pulled back before an approach button can be pressed!";
                yield break;
            }
            else if (duckParts[0].gameObject.activeSelf)
            {
                yield return "sendtochaterror The duck has already been approached!";
                yield break;
            }
            btnSelectables[2].OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*br\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*bottomright\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*bottom-right\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (curtainSelectable.enabled)
            {
                yield return "sendtochaterror The curtain must be pulled back before an approach button can be pressed!";
                yield break;
            }
            else if (duckParts[0].gameObject.activeSelf)
            {
                yield return "sendtochaterror The duck has already been approached!";
                yield break;
            }
            btnSelectables[3].OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*belly\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*afro\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*beak\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*left\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*eye\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*tail\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*right\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            List<string> parts = new List<string>() { "belly", "afro", "beak", "left", "right", "eye", "tail" };
            if (curtainSelectable.enabled)
            {
                yield return "sendtochaterror The curtain must be pulled back before a duck part can be clicked!";
                yield break;
            }
            else if (!duckParts[0].gameObject.activeSelf)
            {
                yield return "sendtochaterror The duck has not been approached yet!";
                yield break;
            }
            duckParts[parts.IndexOf(command.ToLower())].OnInteract();
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (curtainSelectable.enabled)
        {
            curtainSelectable.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        if (!duckParts[0].gameObject.activeSelf)
        {
            for (int i = 0; i < 4; i++)
            {
                if (shownApproaches[i] == correctApproach)
                {
                    btnSelectables[i].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                    break;
                }
            }
        }
        duckParts[correctApproach].OnInteract();
    }
}
