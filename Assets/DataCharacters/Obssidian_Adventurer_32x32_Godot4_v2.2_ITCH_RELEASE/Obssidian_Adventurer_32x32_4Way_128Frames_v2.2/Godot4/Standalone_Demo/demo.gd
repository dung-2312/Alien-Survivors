extends Node2D

@onready var actor: Node2D = $Actor
@onready var sprite: AnimatedSprite2D = $Actor/Adventurer
var direction := "south"
var locked_action := ""
var dead := false
var jump_tween: Tween

func _ready() -> void:
    sprite.animation_finished.connect(_on_animation_finished)

func play_once(action: String) -> void:
    if dead and action != "death":
        return
    locked_action = action
    sprite.play(action + "_" + direction)

func play_jump() -> void:
    if locked_action != "" or dead:
        return
    play_once("jump")
    if jump_tween and jump_tween.is_valid():
        jump_tween.kill()
    sprite.position.y = 0.0
    jump_tween = create_tween()
    jump_tween.set_trans(Tween.TRANS_QUAD)
    jump_tween.set_ease(Tween.EASE_OUT)
    jump_tween.tween_property(sprite, "position:y", -28.0, 0.28)
    jump_tween.set_ease(Tween.EASE_IN)
    jump_tween.tween_property(sprite, "position:y", 0.0, 0.32)

func _unhandled_input(event: InputEvent) -> void:
    if event.is_action_pressed("ui_accept"):
        play_jump()
    elif event is InputEventKey and event.pressed and not event.echo:
        match event.keycode:
            KEY_B: play_once("blink")
            KEY_C: play_once("crouch")
            KEY_H: play_once("hurt")
            KEY_K:
                dead = true
                play_once("death")
            KEY_R:
                dead = false
                locked_action = ""
                sprite.position.y = 0.0
                sprite.play("idle_" + direction)

func _process(delta: float) -> void:
    if locked_action != "" or dead:
        return
    var v := Input.get_vector("ui_left", "ui_right", "ui_up", "ui_down")
    if abs(v.x) > abs(v.y):
        direction = "east" if v.x > 0 else "west"
    elif abs(v.y) > 0.0:
        direction = "south" if v.y > 0 else "north"
    var action := "walk" if v.length() > 0.0 else "idle"
    sprite.play(action + "_" + direction)
    actor.position += v * 55.0 * delta
    actor.position.x = clamp(actor.position.x, 70.0, 570.0)
    actor.position.y = clamp(actor.position.y, 135.0, 280.0)

func _on_animation_finished() -> void:
    if locked_action == "death":
        sprite.pause()
        return
    locked_action = ""
    sprite.play("idle_" + direction)
