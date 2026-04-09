extends Node2D

var item_id := "ammo_556"

func _ready() -> void:
	var rect := ColorRect.new()
	rect.color = Color(0.92, 0.92, 0.2, 1)
	rect.rect_size = Vector2(16, 16)
	rect.rect_position = Vector2(-8, -8)
	add_child(rect)

