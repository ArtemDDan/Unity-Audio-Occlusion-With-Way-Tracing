# Unity-Scripts-For-Audio-Implementation

# Spline sound source mover
NEW
The script is used for moving the sound source along the spline relative to the player's position. 
Such setup is used for adding sounds to rivers or other large curvy objects that emit the sound. 
This script requiers the latest spline package by Unity.




# Sound occlusion script for Unity using Ray Casting
Ver 2.0 
Update Log:
- No need to add low-pass filter on object anymore, it's done automatically;
- No need to add instance of the script on each object. Placing one on a parent is enough;
- Auto assign Listener Mode by tag. Solves the problem with using this script on prefabs;
- Instant apply mode. Don't want gradual interpolation for sounds that spawn already behind the obstacle? Possible now.

Ver 1.0 
Basic setup:
- Attach an Audio Source(s), Audio Low Pass Filter components  to your object;
- Attach the script to the object;
- Set all the public variables in the inspector.
