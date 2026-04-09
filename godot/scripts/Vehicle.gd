extends KinematicBody2D

var occupied := false
var speed := 240.0

func drive_from_input(delta: float) -> void:
	var dir := Vector2.ZERO
	if Input.is_action_pressed("move_forward"):
		dir.y -= 1
	if Input.is_action_pressed("move_back"):
		dir.y += 1
	if Input.is_action_pressed("move_left"):
		dir.x -= 1
	if Input.is_action_pressed("move_right"):
		dir.x += 1
	dir = dir.normalized()
	move_and_slide(dir * speed)

