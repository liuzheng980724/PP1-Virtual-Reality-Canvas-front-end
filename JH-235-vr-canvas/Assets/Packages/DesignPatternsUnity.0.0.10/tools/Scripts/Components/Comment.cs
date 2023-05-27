using UnityEngine;

/* 
 * This component allows developers to add comments to gameobjects in scene.
 * Be aware, comments made in this component are stored in the scene file 
 * and are plaintext readable in build and source. Do not store usernames, 
 * passwords, api keys, etc in these components.
 */
public class Comment : MonoBehaviour
{
    [TextArea(1, 1)]
    [Tooltip("Doesn't do anything. Just the author name shown in inspector")]
    public string Author = "Author name";
    [TextArea(5,20)]
    [Tooltip("Doesn't do anything. Just comments shown in inspector")]
    public string Notes = "Add notes for other developers here";
}
