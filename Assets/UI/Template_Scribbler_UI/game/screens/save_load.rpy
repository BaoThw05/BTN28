## Load and Save screens #######################################################
##
## These screens are responsible for letting the player save the game and load
## it again. Since they share nearly everything in common, both are implemented
## in terms of a third screen, file_slots.
##
## https://www.renpy.org/doc/html/screen_special.html#save
## https://www.renpy.org/doc/html/screen_special.html#load


## The width and height of thumbnails used by the save slots.
define config.thumbnail_width = 265
define config.thumbnail_height = 146


screen save():

    tag menu

    use file_slots(_("Save"))


screen load():

    tag menu

    use file_slots(_("Load"))

screen file_slots(title):

    default page_name_value = FilePageNameInputValue(
        pattern=_("Page {}"), auto=_("Autosaves"),
        quick=_("Quick saves"))

    use game_menu(title)

    fixed:
        xsize 500 xalign 0.5
        
        ## This ensures the input will get the enter event before any of the
        ## buttons do.
        order_reverse True

        ## The page name, which can be edited by clicking on it.
        ## This can be pretty easily removed if you want.
        ## Don't forget to also remove the `default` at the top if so.
        button:
            style "page_label"
            key_events True
            action page_name_value.Toggle()

            input:
                style "page_label_text"
                value page_name_value


        #fancy tape decoration that i stole the code from frangame for. thanks past me.
        grid 1 2:
            yoffset 20
            style_prefix "slot"

            #TODO: ideally randomise these tapes. but also who cares.
            for i in range(1*2):
                $ slot = i + 1
                add "gui/button/tape_[i].png"

        ## The grid of file slots.
        grid 1 2:
            yoffset 20
            style_prefix "slot"

            for i in range(1*2):
                $ slot = i + 1

                button:
                    action FileAction(slot)
                    has vbox

                    add FileScreenshot(slot) xalign 0.5

                    ## https://www.fabriziomusacchio.com/blog/2021-08-15-strftime_Cheat_Sheet/
                    text FileTime(slot,
                            format=_("{#file_time}%B %d %Y, %H:%M"),
                            empty=_("empty slot")):
                        style "slot_time_text"
                        yoffset -7

                    key "save_delete" action FileDelete(slot)
        ## Buttons to access other pages.
        vbox:
            yoffset 10
            xoffset 2
            style_prefix "page"
            hbox:
                textbutton _("<") action FilePagePrevious()

                if config.has_autosave:
                    textbutton _("{#auto_page}A") action FilePage("auto")

                if config.has_quicksave:
                    textbutton _("{#quick_page}Q") action FilePage("quick")

                ## range(1, 10) gives the numbers from 1 to 9.
                for page in range(1, 5):
                    textbutton "[page]" action FilePage(page)

                textbutton _(">") action FilePageNext()



style page_label:
    xpadding 75
    ypadding 5
    xalign 0.5
    yoffset 210
    xoffset 180

style page_label_text:
    textalign 0.5
    layout "subtitle"

style slot_grid:
    xalign 0.5
    yalign 0.5
    spacing 15

style slot_time_text:
    size 24
    xalign 0.5
    font "fonts/RandoArtline.ttf"
    line_leading 0
    line_spacing -4

style slot_vbox:
    spacing 12

style slot_button:
    xysize (295, 224)
    padding (15, 15, 15, 15)
    background "gui/button/slot_[prefix_]background.png"


style slot_button_text:
    size 21
    xalign 0.5

style page_hbox:
    xalign 0.5
    spacing 5

style page_vbox:
    xalign 0.5
    ypos 850
    spacing 5

style page_button:
    padding (15, 6, 15, 6)
    xalign 0.5

# style page_button_text:

