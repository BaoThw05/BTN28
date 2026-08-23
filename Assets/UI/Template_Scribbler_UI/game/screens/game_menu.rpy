## Game Menu screen ############################################################
##
## This lays out the basic common structure of a game menu screen. It's called
## with the screen title, and displays the title and navigation.
##
## This screen no longer includes a background, and it no longer transcludes
## its contents. It is intended to be easily removable from any given menu
## screen and thus you are required to do some of the heavy lifting for
## setting up containers for the contents of your menu screens.
##

screen game_menu(title):

    style_prefix "game_menu"

    if main_menu:
        add "main_menu_background"
    add "gui/bgtile.png"
    add "gui/menubook.png"
    vbox:
        yalign 0.5
        xalign 1.0
        spacing 5
        xoffset -520

        if main_menu:

            textbutton _("Start") action Start()
            null height 20

        else:

            textbutton _("Log") action ShowMenu("history")
            null height 20

            textbutton _("Save") action ShowMenu("save")

        textbutton _("Load") action ShowMenu("load")
        null height 20

        textbutton _("Display") action ShowMenu("display_prefs") text_size 33
        textbutton _("Audio") action ShowMenu("audio_prefs")
        null height 20


        textbutton _("About") action ShowMenu("about")

        textbutton _("Help") action ShowMenu("help")

        if not main_menu:
            textbutton _("Quit") action MainMenu() 

    imagebutton auto "gui/return_%s.png" action Return() focus_mask True

    ## Remove this line if you don't want to show the screen
    ## title text as a label (for example, if it's baked into
    ## the background image.)
    label title

    if main_menu:
        key "game_menu" action ShowMenu("main_menu")

style return_button:
    xpos 60
    yalign 1.0
    yoffset -45

style game_menu_viewport:
    xsize 560
    ysize 590
    xoffset 50
    ypos 30
    # align (0.5, 0.5)

style game_menu_side:
    # yfill True
    align (0.5, 0.5)

style game_menu_vscrollbar:
    unscrollable "hide"
    yoffset 15

style game_menu_label:
    padding (10, 10)
    pos (650,177)


style game_menu_label_text:
    is main_menu_button_text
    size 60
    color "#E47751"


style game_menu_button:
    idle_background "gui/menusticky_idle.png"
    hover_background "gui/menusticky_hover.png"

style game_menu_button_text:
    is main_menu_button_text
    size 35
    xoffset 10
    yoffset 0
    selected_color '#D26143'
    idle_color "#D26143"
    insensitive_color "#778288"
    hover_color "#E99067"

style label_text:
    is text
    font "RandoSharpie.ttf"
    size 45
