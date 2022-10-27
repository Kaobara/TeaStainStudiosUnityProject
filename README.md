

**The University of Melbourne**
# COMP30019 – Graphics and Interaction

## Teamwork plan/summary

<!-- [[StartTeamworkPlan]] PLEASE LEAVE THIS LINE UNTOUCHED -->

<!-- Fill this section by Milestone 1 (see specification for details) -->

We will be using Confluence to keep track of our weekly progress and organising all of our intellectual work. We will be having weekly meeting to ensure smooth discussions and to report to Project Manager, Mukul. Weekly, Fortnightly and Montly goals will be set to ensure each Deliverable is submitted.
|Role | Who |
| ----------- | ----------- |
| Physics | Sebastian |
| Gameplay | Mukul, Sebastian, Daniel, Jason |
| General Pixel Shaders | Sebastion & Daniel |
| Particle| Daniel |
| Animation | Daniel and Seastian|
| Objs and Assets | Mukul |
| Music, SFX and Sound Assets | Jason |
| Procedural generation (scenery + maps and clouds) | Mukul
| Camera Orientation and Controlling | Jason |
| Querying and Playtesting and Surveying | Jason and Mukul |
| Level Design | Mukul and everyone |
| Storytelling and Objectives | Daniel and Mukul |


<!-- [[EndTeamworkPlan]] PLEASE LEAVE THIS LINE UNTOUCHED -->

## Final report

Read the specification for details on what needs to be covered in this report... 

Remember that _"this document"_ should be `well written` and formatted **appropriately**. 
Below are examples of markdown features available on GitHub that might be useful in your report. 
For more details you can find a guide [here](https://docs.github.com/en/github/writing-on-github).

### Table of contents
* [Game Summary](#game-summary)
* [How To Play The Game] (#how-to-play-the-game)
* [Technologies](#technologies)
* [Using Images](#using-images)
* [Code Snipets](#code-snippets)

### Game Summary
_Exciting title_ is a first-person shooter (FPS) set in...

### How To Play The Game
As briefly outlined, Chiki’s Delivery Service is a game where players traverse a level to pick up packages and then deliver them to the goal point as fast as possible. To do so, players can utilise the terrain and gameplay mechanics. Using Chiki’s strong legs and wings, players can jump up objects that have more height, allowing them to reach further places or perhaps even reach places faster through the glide mechanic.

Standard WASD movement and spacebar jump is used to control Chiki. Additionally, players can also hold spacebar when Chiki is in the air to glide through the air. Be careful of falling in the water though as Chiki is not a good swimmer. Falling in the water means Chiki has to dry itself and start again!

Clearing a level awards the player (and Chiki) a bronze medal for the level. In order to achieve silver medals and gold medals, players will need to utilise their proficiency in gameplay mechanics and knowledge of the level in order to deliver the package and complete the level within a shorter duration of time. After all, customers always want their packages to be delivered quickly.


### Technologies
Project is created with:
* Unity 2022.1.9f1 
* Ipsum version: 2.33
* Ament library version: 999

### Using Images

You can include images/gifs by adding them to a folder in your repo, currently `Gifs/*`:

<p align="center">
  <img src="Gifs/sample.gif" width="300">
</p>

To create a gif from a video you can follow this [link](https://ezgif.com/video-to-gif/ezgif-6-55f4b3b086d4.mov).

### Code Snippets 

You may wish to include code snippets, but be sure to explain them properly, and don't go overboard copying
every line of code in your project!

```c#
public class CameraController : MonoBehaviour
{
    void Start ()
    {
        // Do something...
    }
}
```
