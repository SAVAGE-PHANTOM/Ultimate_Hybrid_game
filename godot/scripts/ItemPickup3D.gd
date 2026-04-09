extends Area

export(String) var item_id := "ammo_556"
export(int) var amount := 1

func _ready() -> void:
	connect("body_entered", self, "_on_body_entered")

func _on_body_entered(body: Node) -> void:
	if body == null:
		return
	if not body.has_method("add_item"):
		return
	body.add_item(item_id, amount)
	queue_free()

