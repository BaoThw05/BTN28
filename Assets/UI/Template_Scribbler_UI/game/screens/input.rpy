
## Input screen ################################################################
##
## This screen is used to display renpy.input. The prompt parameter is used to
## pass a text prompt in.
##
## This screen must create an input displayable with id "input" to accept the
## various input parameters.
##
## https://www.renpy.org/doc/html/screen_special.html#input

#optional; but you need this for the onscreen button to work and not just use Enter as your grab input button
init -2 python:
    class GetText(Action):
        def __init__(self,screen_name,input_id):
            self.screen_name=screen_name
            self.input_id=input_id
        def __call__(self):
            if renpy.get_widget(self.screen_name,self.input_id):
                return str(renpy.get_widget(self.screen_name,self.input_id).content)


screen input(prompt):
    style_prefix "input"
        
    add "gui/bgtile.png" 
    add "gui/menubook.png" 

    vbox:
        xalign 0.5
        ypos 0.35
        xsize 550
        xoffset 40
        text prompt style "input_prompt"
        null height 30
        input id "input" color "D26143"

        textbutton "   Okay!" action GetText("input","input"):  
            text_font "fonts/RandoWB.ttf"
            text_size 50
            text_selected_color '#D26143'
            text_idle_color "#D26143"
            text_insensitive_color "#778288"
            text_hover_color "#008DBF"
            text_yoffset 10
            hover_background "gui/mm_hl.png"

        null height 50


style input_prompt:
    xalign 0.0
    color "#00B1E1"

style input:
    xalign 0.0
    xmaximum 550


