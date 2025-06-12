using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class animationController : MonoBehaviour
{
    Animator entityAnimator;
    string currentAnimationName;
    /*
     * NOTE: THIS IS UNFINISHED BUT THE SCRIPT NEEDS TO HANDLE CHANGING THE ANIMATION AFTER OTHER SCRIPTS CHANGE CURRENTANIMATIONNUM TO KINDA REFLECT THE MODEL'S STATE
     * THIS IS SUPPOSED TO BE MORE GENERIC SO THAT YOU CAN LIKE DO ISOMORPHISM SHENANIGANS AND FIT IT ON THE PLAYER, GUARDS AND NPCS
     * 
     * */
    //public int currentAnimationNum;
    //public string filePathTemp;
    //the animation name list is taking the name of the parent folder (save for the movement stuff) because of the way the animations are all organized
    //aka idles only have one animation so its not ambiguous but the movement and socials folder is quite ambiguous
    private string[] animationNameList = { "Idles", "Jump", "Run", "Sprint", "Turn", "Walk", "Conversation" };
    // Start is called before the first frame update
    void Start()
    {
        entityAnimator = GetComponent<Animator>();
        currentAnimationName = "Assets/Kevin Iglesias/Human Animations/Unity Demo Scenes/Basic Motions Scene/Animator Controllers/HumanM-Basic@Idles";
        entityAnimator.runtimeAnimatorController = Resources.Load(currentAnimationName) as RuntimeAnimatorController;

    }
    private void FixedUpdate()
    {

        /*if (currentAnimationNum > 0 && currentAnimationNum < 6)
        {
            currentAnimationName += "Movement/";
        }
        else if (currentAnimationNum == 6)
        {
            currentAnimationName += "Social/";
        }
        currentAnimationName += animationNameList[currentAnimationNum];*/
        

        //Debug.Log(currentAnimationName);
    }

}