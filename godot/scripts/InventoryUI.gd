extends Control

var player_ref := null
onready var panel := $Panel
onready var items := $Panel/Items

func bind_player(p) -> void:
	player_ref = p

func _process(_delta: float) -> void:
	if Input.is_action_just_pressed("inventory"):
		panel.visible = !panel.visible

func _on_inventory_changed(inventory_text: String) -> void:
	items.text = inventory_text

