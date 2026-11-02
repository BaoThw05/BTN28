
## About screen ################################################################
##
## This screen gives credit and copyright information about the game and Ren'Py.
##
## There's nothing special about this screen, and hence it also serves as an
## example of how to make a custom screen.

## Text that is placed on the game's about screen. Place the text between the
## triple-quotes, and leave a blank line between paragraphs.

define gui.about = _p("""A visual novel GUI kit created by Madi Wander {a=https://timepatches.info}@timepatches{/a}.\n
All assets (including UI assets, Kibby catgirl sprite, and sky background) made by the creator by hand, and made available for use under license.

Created using Fenik's {a=https://github.com/shawna-p/EasyRenPyGui}EasyRen'pyGUI{/a}. You must retain the credit for EasyRenPyGUI in anything you create using the code in this kit.
                      
Fonts:

{a=https://beaumaher.gumroad.com/l/rando-sans}Rando Sans (variable){/a} by Beau Maher - an indie creator, so I recommend throwing them a few dollars if you're able!

{a=https://monaspace.githubnext.com/}Monaspace (Neon){/a}
""")


screen about():

    tag menu

    use game_menu(_("About"))

    viewport:
        style_prefix 'game_menu'
        mousewheel True draggable True pagekeys True
        scrollbars "vertical"

        has vbox
        style_prefix "about"

        label "[config.name!t]"
        text _("Version [config.version!t]\n")

        if gui.about:
            text "[gui.about!t]\n" xsize 500


        text _("Made with {a=https://www.renpy.org/}Ren'Py{/a} [renpy.version_only].\n\n[renpy.license!t]")


style about_label_text:
    size 36

style about_text:
    font "fonts/MonaspaceNeon-Regular.otf"
    color "#DB8D7A"
    size 23

## Help screen #################################################################
##
## A screen that gives information about key and mouse bindings. It uses other
## screens (keyboard_help, mouse_help, and gamepad_help) to display the actual
## help.

screen help():

    tag menu

    default device = "keyboard"

    use game_menu(_("Help"))

    viewport:
        xsize 550
        style_prefix 'game_menu'
        mousewheel True draggable True pagekeys True
        scrollbars "vertical"

        has vbox
        style_prefix "help"
        spacing 23

        hbox:

            textbutton _("Keyboard") action SetScreenVariable("device", "keyboard")
            textbutton _("Mouse") action SetScreenVariable("device", "mouse")

            if GamepadExists():
                textbutton _("Gamepad") action SetScreenVariable("device", "gamepad")

        if device == "keyboard":
            use keyboard_help
        elif device == "mouse":
            use mouse_help
        elif device == "gamepad":
            use gamepad_help

        null height 25


screen keyboard_help():

    hbox:
        label _("Enter")
        text _("Advances dialogue and activates the interface.")

    hbox:
        label _("Space")
        text _("Advances dialogue without selecting choices.")

    hbox:
        label _("Arrow Keys")
        text _("Navigate the interface.")

    hbox:
        label _("Escape")
        text _("Accesses the game menu.")

    hbox:
        label _("Ctrl")
        text _("Skips dialogue while held down.")

    hbox:
        label _("Tab")
        text _("Toggles dialogue skipping.")

    hbox:
        label _("Page Up")
        text _("Rolls back to earlier dialogue.")

    hbox:
        label _("Page Down")
        text _("Rolls forward to later dialogue.")

    hbox:
        label "H"
        text _("Hides the user interface.")

    hbox:
        label "S"
        text _("Takes a screenshot.")

    hbox:
        label "V"
        text _("Toggles assistive {a=https://www.renpy.org/l/voicing}self-voicing{/a}.")

    hbox:
        label "Shift+A"
        text _("Opens the accessibility menu.")


screen mouse_help():

    hbox:
        label _("Left Click")
        text _("Advances dialogue and activates the interface.")

    hbox:
        label _("Middle Click")
        text _("Hides the user interface.")

    hbox:
        label _("Right Click")
        text _("Accesses the game menu.")

    hbox:
        label _("Mouse Wheel Up")
        text _("Rolls back to earlier dialogue.")

    hbox:
        label _("Mouse Wheel Down")
        text _("Rolls forward to later dialogue.")


screen gamepad_help():

    hbox:
        label _("Right Trigger\nA/Bottom Button")
        text _("Advances dialogue and activates the interface.")

    hbox:
        label _("Left Trigger\nLeft Shoulder")
        text _("Rolls back to earlier dialogue.")

    hbox:
        label _("Right Shoulder")
        text _("Rolls forward to later dialogue.")


    hbox:
        label _("D-Pad, Sticks")
        text _("Navigate the interface.")

    hbox:
        label _("Start, Guide, B/Right Button")
        text _("Accesses the game menu.")

    hbox:
        label _("Y/Top Button")
        text _("Hides the user interface.")

    textbutton _("Calibrate Gamepad") action GamepadCalibrate() xoffset 5 text_size 30





style help_button:
    xmargin 12
    xoffset -15
style help_button_text:
    size 35


style help_label:
    xsize 150
    right_padding 30

style help_label_text:
    xalign 0.0
    textalign 0.0
    size 30
    yalign 0.5

style help_text:
    color "#DB8D7A"
    size 20
    font "fonts/MonaspaceNeon-Regular.otf"
    yalign 0.5
