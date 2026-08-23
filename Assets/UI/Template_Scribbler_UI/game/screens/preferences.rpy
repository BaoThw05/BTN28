
## Preferences screen ##########################################################
##
## The preferences screen allows the player to configure the game to better suit
## themselves.
##
## https://www.renpy.org/doc/html/screen_special.html#preferences

screen display_prefs():

    tag menu

    use game_menu(_("Display Settings"))

    viewport:
        
        style_prefix 'game_menu'
        mousewheel True pagekeys True
        scrollbars "vertical"
        has vbox

        hbox:
            box_wrap True
        
            vbox:
                style_prefix "radio"
                label _("Screen Mode")
                textbutton _("Window"):
                    # Ensures this button is selected when
                    # not in fullscreen.
                    selected not preferences.fullscreen
                    action Preference("display", "window")
                textbutton _("Fullscreen"):
                    action Preference("display", "fullscreen")

            vbox:
                style_prefix "check"
                label _("While Skipping:")
                textbutton _("Skip Unseen Text"):
                    action Preference("skip", "toggle")
                textbutton _("Skip After Choices"):
                    action Preference("after choices", "toggle")
                null height 20

            vbox:
                style_prefix "radio"
                label _("Auto-Forward Mode")
                text "Allow text to auto-advance without clicking." size 16 font "fonts/MonaspaceArgon-SemiBold.otf" color "#00719A"
                textbutton _("Enabled") action Preference("auto-forward", "enable") 
                textbutton _("Disabled") action Preference("auto-forward", "disable") 

            vbox:
                style_prefix "slider"
                box_wrap True
                yoffset -15
                label _("Auto-Forward Time")
                
                bar value Preference("auto-forward time")
                null height 5
                text "Leftmost is fastest." size 16 font "fonts/MonaspaceArgon-SemiBold.otf" color "#00719A"
                
                null height 15
                label _("Text Speed")
                bar value Preference("text speed")

            null height 50
                
            ## Additional vboxes of type "radio_pref" or "check_pref" can be
            ## added here, to add additional creator-defined preferences.

screen audio_prefs():
    tag menu

    use game_menu(_("Audio Settings"))

    viewport:
        style_prefix 'game_menu'
        scrollbars "vertical"
        has vbox

        vbox:
            yoffset 40
            spacing 3
            style_prefix "slider"
            box_wrap True
            
            if config.has_music:
                label _("Music Volume")
                hbox:
                    bar value Preference("music volume")

            if config.has_sound:
                label _("Sound Volume")
                hbox:
                    bar value Preference("sound volume")
                    if config.sample_sound:
                        textbutton _("Test") action Play("sound", config.sample_sound)


            if config.has_voice:
                label _("Voice Volume")
                hbox:
                    bar value Preference("voice volume")
                    if config.sample_voice:
                        textbutton _("Test") action Play("voice", config.sample_voice)

            if config.has_music or config.has_sound or config.has_voice:
                null height 15
                textbutton _("Mute All"):
                    style_prefix "check"
                    action Preference("all mute", "toggle")



### PREF
style pref_label:
    top_margin 15
    bottom_margin -8

style pref_label_text:
    yalign 1.0
    # outlines [ (absolute(3), "#ffffffff", 0, 0) ]

style pref_vbox:
    xsize 450

## RADIO
style radio_label:
    is pref_label

style radio_label_text:
    is pref_label_text

style radio_vbox:
    is pref_vbox
    spacing 0

style radio_button:
    foreground "gui/button/radio_[prefix_]foreground.png"
    padding (35, 6, 6, 6)

style radio_button_text:
    outlines [ (absolute(3), "#ffffffff", 0, 0) ]

## CHECK
style check_label:
    is pref_label
style check_label_text:
    is pref_label_text

style check_vbox:
    is pref_vbox
    spacing 0

style check_button:
    foreground "gui/button/check_[prefix_]foreground.png"
    padding (35, 6, 6, 6)

style check_button_text:
    outlines [ (absolute(3), "#ffffffff", 0, 0) ]

## SLIDER
style slider_label:
    is pref_label
    bottom_margin 5
style slider_label_text:
    is pref_label_text

style slider_slider:
    xsize 400

style slider_button:
    yalign 0.5
    left_margin 15

style slider_vbox:
    is pref_vbox
    xsize 675

