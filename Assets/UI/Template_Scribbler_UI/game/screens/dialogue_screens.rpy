
## Say screen ##################################################################
##
## The say screen is used to display dialogue to the player. It takes two
## parameters, who and what, which are the name of the speaking character and
## the text to be displayed, respectively. (The who parameter can be None if no
## name is given.)
##
## This screen must create a text displayable with id "what", as Ren'Py uses
## this to manage text display. It can also create displayables with id "who"
## and id "window" to apply style properties.
##
## https://www.renpy.org/doc/html/screen_special.html#say

transform multiply:
    blend "multiply"

screen say(who, what):
    style_prefix "say"
    add "gui/textbox.png"
    window:
        
        id "window"

        if who is not None:

            window:
                id "namebox"
                style "namebox"
                text who id "who"
                at multiply

        text what id "what"

    if quick_menu:
        frame:
            background None
            add "qmbook"
            ypos 720
            xpos 200
            vbox:
                yoffset 10
                xoffset 50
                spacing 9
                at rotated
                style_prefix "quick"
                hbox:
                    textbutton _("<<") action Rollback() hover_background "gui/hlsmall.png"
                    textbutton _(">>") action Skip() alternate Skip(fast=True, confirm=True) hover_background "gui/hlsmall.png"
                    textbutton _("Auto") action Preference("auto-forward", "toggle")
                    
                hbox:
                    textbutton _("Save") action ShowMenu('save')
                    textbutton _("Log") action ShowMenu('history')
                hbox:
                    textbutton _("Settings") action ShowMenu('display_prefs') hover_background "gui/hlbig.png"
    ## If there's a side image, display it in front of the text.
    add SideImage() xalign 0.0 yalign 1.0


## Make the namebox available for styling through the Character object.
init python:
    config.character_id_prefixes.append('namebox')

# Style for the dialogue window
style window:
    xalign 0.0
    yalign 1.0
    xysize (999, 277)
    
    padding (40, 10, 40, 40)

# Style for the dialogue
style say_dialogue:
    adjust_spacing False
    ypos 50
    xpos 650
    color "#D26143"
    outlines [ (absolute(2), "#ffffffff", 0, 0) ]
    font "MonaspaceNeon-Regular.otf"
    line_leading 0
    line_spacing 13
    size 32

# The style for dialogue said by the narrator
style say_thought:
    is say_dialogue

# Style for the box containing the speaker's name
style namebox:
    xpos 600
    yoffset -20
    xysize (None, None)
    background Frame("gui/namebox.png", 5, 5, 5, 5, tile=False, xalign=0.0)
    padding (25, 5, 50, 10)

# Style for the text with the speaker's name
style say_label:
    xalign 0.0
    yalign 0.5
    font "RandoTexta.ttf"
    size 52
    color "#008DBF"
    


## Quick Menu screen ###########################################################
##
## The quick menu is displayed in-game to provide easy access to the out-of-game
## menus.

image qmbook:
    "gui/qmbook.png"



transform rotated:
    rotate -18


# screen quick_menu():

#     ## Ensure this appears on top of other screens.
#     zorder 100


                    


# ## This code ensures that the quick_menu screen is displayed in-game, whenever
# ## the player has not explicitly hidden the interface.
# init python:
#     config.overlay_screens.append("quick_menu")

default quick_menu = True

# style quick_hbox:
#     xalign 0.5
#     yalign 1.0 yoffset -8
#     spacing 8

style quick_button:
    idle_background "gui/none.png"
    hover_background "gui/hl1.png"
    selected_background "gui/none.png"
 
    padding (15, 6, 15, 0)


style quick_button_text:
    font "fonts/RandoWB.ttf"
    size 40

    selected_color '#D26143'
    idle_color "#E99067"
    insensitive_color "#a6d2dd"
    hover_color "#008DBF"

## NVL screen ##################################################################
##
## This screen is used for NVL-mode dialogue and menus.
##
## https://www.renpy.org/doc/html/screen_special.html#nvl


screen nvl(dialogue, items=None):

    window:
        style "nvl_window"

        has vbox
        spacing 15

        use nvl_dialogue(dialogue)

        ## Displays the menu, if given. The menu may be displayed incorrectly if
        ## config.narrator_menu is set to True.
        for i in items:

            textbutton i.caption:
                action i.action
                style "nvl_button"

    add SideImage() xalign 0.0 yalign 1.0


screen nvl_dialogue(dialogue):

    for d in dialogue:

        window:
            id d.window_id

            fixed:
                yfit True

                if d.who is not None:

                    text d.who:
                        id d.who_id

                text d.what:
                    id d.what_id


## This controls the maximum number of NVL-mode entries that can be displayed at
## once.
define config.nvl_list_length = 6

# The style for the NVL "textbox"
style nvl_window:
    is default
    xfill True yfill True
    background "gui/nvl.png"
    padding (0, 15, 0, 30)

# The style for the text of the speaker's name
style nvl_label:
    is say_label
    xpos 645 xanchor 1.0
    ypos 0 yanchor 0.0
    xsize 225
    min_width 225
    textalign 1.0

# The style for dialogue in NVL
style nvl_dialogue:
    is say_dialogue
    xpos 675
    ypos 12
    xsize 885
    min_width 885

# The style for dialogue said by the narrator in NVL
style nvl_thought:
    is nvl_dialogue

style nvl_button:
    xpos 675
    xanchor 0.0

