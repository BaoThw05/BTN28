
## Main Menu screen ############################################################
##
## Used to display the main menu when Ren'Py starts.
##
## https://www.renpy.org/doc/html/screen_special.html#main-menu


image main_menu_background = "images/bg sky.png"
## Replace this with y"our background image, if you like


screen main_menu():

    ## This ensures that any other menu screen is replaced.
    tag menu

    add "main_menu_background"
    add "gui/sidefade.png" #you can remove this if your main menu image accommodates the buttons 

    vbox:
        style_prefix "main_menu"
        
        xalign 0.9
        yalign 0.5
        spacing 20

        textbutton _("Start") action Start()

        textbutton _("Load") action ShowMenu("load")

        textbutton _("Settings") action ShowMenu("display_prefs")    hover_background "gui/mm_hl2.png"

        textbutton _("About") action ShowMenu("about")

        if renpy.variant("pc") or (renpy.variant("web") and not renpy.variant("mobile")):

            ## Help isn't necessary or relevant to mobile devices.
            textbutton _("Help") action ShowMenu("help")

        if renpy.variant("pc"):

            ## The quit button is banned on iOS and unnecessary on Android and
            ## Web.
            textbutton _("Quit") action Quit(confirm=not main_menu)


style main_menu_button_text:
    text_align 1.0
    
    font "fonts/RandoWB.ttf"
    size 50
    selected_color '#D26143'
    idle_color "#D26143"
    insensitive_color "#778288"
    hover_color "#008DBF"

    # xcenter 0.5 
    # ycenter 0.5

    xoffset 50
    yoffset 10
    

style main_menu_button:
    text_align 1.0
    xalign 1.0
    hover_background "gui/mm_hl.png"
