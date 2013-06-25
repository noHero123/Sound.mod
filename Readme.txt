Hy, 
here is my dirty little mod.
It allows you to play your beloved music (mp3 and/or wav) at beginning and the end of your own turn.
Here it is: Githublink yay

compiled dll: http://www31.zippyshare.com/v/82623600/file.html

how to use: place two sound files in the .../Mojang/Scrolls/game/ directory
(where the Scrolls.exe and the folder "Scrolls_Data" is located) and rename these sound files as follows:
 the sound which should be played at the start of your turn has to be renamed in "scrolls_startturn.mp3" (or .wav).

the sound which should be played in the end of your turn has to be 
renamed in "scrolls_endturn_NUMBER.mp3" (or .wav).
where 0 <= NUMBER <= 90  
this file will be played after 90 - Number seconds of the beginning of your turn.
example: the file "scrolls_endturn_36.wav" will be played after 54 seconds
or better: it starts to play, when the timer shows 36 seconds. 
so you can adjust the start of the "endturn-song" by yourself. (you have to restart the game if you rename the scrolls_endturn_NUMBER.xxx file).
