# Music System Documentation

### updateMusic(int character, int topic, float tension, float awkwardness, float resolution, bool interruption)

With this function, you can call the music system to queue up the next appropriate music asset. It can be called as many times as you like -- the queue will simply grow until the music is finished playing. If an interrupt is called, the music queue is flushed, music is abruptly stopped, and then begins again after a pause.

To indicate the current player speaking:  
    character  
    0 = Realist (player character)  
    1 = Dreamer

To indicate the current topic:  
    topic  
    0 = introduction  
    1 = careers  
    2 = childhood memories  
    3 = relationships  

For the conversation's tension:  
    tension  
    0 = low tension  
    1 = high tension  

For the conversation's awkwardness:  
    awkwardness  
    0 = low awkwardness  
    1 = high awkwardness  

For the resolution between the two characters:  
    resolution  
    0 = low resolution  
    1 = high resolution  

To indicate if there was an interruption:  
    interrupt  
    false = no interruption (duh)  
    true = interruption  
