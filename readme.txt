Setup:
 This shader requires 1 particular thing to not look terrible, and
 that would be a customized font. In Unity, it should be enough to 
 import a font file directly to the project (all OS installed fonts
 for Windows users are located at C:/Windows/Fonts, if that helps).

 Before continuing, it's worth noting that you can probably skip this
 until you've finally settled on the final font(s) to be used, since
 the custom font step is mainly to get rid of a rendering artifact
 that you won't want in your final version. No need to repeat this step
 for every font you're considering.

 From there, the font's settings panel should have a CHARACTER select
 option. Set this to anything besides 'Dynamic', and the locate the
 gear symbol up in the top right. From there, you should get an option
 to 'create editable copy.' Do that, find the copied font, open its
 settings, and increase the Character Padding value by some amount,
 probably 2 or 3. This can be changed later if need be.


Usage:
 Any Text Entities should have an option near the end of the Text
 Component called Material. Set this to be 'ShakyTextMaterial', and
 make sure that the shader is set to 'Unlit/TextShakeShader'.

 Add the 'Draw Shaky Text' script to any single-instance entity (such as
 a canvas, or the Camera) and set up the params as desired. The ones
 in the example project seem alright to me, but obviously set them to
 whatever feels right.

 Once you have the shaky text going, you might notice weird lines
 showing up. This is due to how game engines typically generate their
 text, since they don't anticipate any animation. The TLDR is that fonts
 are all dumped into a single sprite sheet, and to maximize efficiency,
 there's not any space put between glyphs on the sheet. This is where
 the previously mentioned Character Padding comes in. Increasing this
 should increase the amount of space between glyphs on the sprite sheet,
 thus removing the lines that come from 'jittering' into a another glyphs
 reserved space.


How It Works:
 There is a shader applied to the ShakyText material that displaces where
 the GPU is looking while drawing the font glyphs. This is done with a
 combo of an ever-changing Seed, and a PNG that acts as a noise map to
 introduce some organic randomization. 

 The material is applied to Text entities, and updated via a script.
 Should be as simple as that!