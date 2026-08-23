
## History screen ##############################################################
##
## This is a screen that displays the dialogue history to the player. While
## there isn't anything special about this screen, it does have to access the
## dialogue history stored in _history_list.
##
## https://www.renpy.org/doc/html/history.html

define config.history_length = 250

screen history():

    tag menu

    ## Avoid predicting this screen, as it can be very large.
    predict False

    use game_menu(_("Log"))

    viewport:
        style_prefix 'game_menu'
        mousewheel True draggable True pagekeys True
        scrollbars "vertical" yinitial 1.0

        has vbox

        style_prefix "history"

        for h in _history_list:

            frame:
                has hbox
                if h.who:
                    label h.who style 'history_name':
                        substitute False
                        ## Take the color of the who text
                        ## from the Character, if set
                        if "color" in h.who_args:
                            text_color h.who_args["color"]
                        xsize 80   # this number and the null width
                                    # number should be the same
                else:
                    null width 80

                $ what = renpy.filter_text_tags(h.what, allow=gui.history_allow_tags)
                text what:
                    substitute False

        if not _history_list:
            label _("The dialogue history is empty.") text_size 35 xoffset -30 yoffset 0 text_outlines [(0, "#ffffff", 0, 0)]


## This determines what tags are allowed to be displayed on the history screen.

define gui.history_allow_tags = { "alt", "noalt", "rt", "rb", "art" }


style history_frame:
    xsize 500
    ysize None
    background None

style history_hbox:
    spacing 15

style history_vbox:
    spacing -50

style history_name:
    xalign 1.0

style history_name_text:
    textalign 1.0
    align (1.0, 0.0)
    color '#5C9D54'
    font "fonts/RandoSharpie.ttf"

style history_text:
    textalign 0.0
    font "fonts/MonaspaceNeon-Regular.otf"
    size 27
    yoffset 10
    color "#b87160"

style history_label:
    xfill True

style history_label_text:
    xalign 0.5
