extends Spatial

onready var network := $Network
onready var world := $World
onready var player_template := $World/PlayerTemplate
onready var inventory_ui := $UI/Inventory
onready var status_bar := $UI/Inventory/StatusBar

func _ready() -> void:
	network.connect("peer_joined", self, "_on_peer_joined")
	network.connect("peer_left", self, "_on_peer_left")
	network.connect("local_player_ready", self, "_on_local_player_ready")
	network.init(player_template, world, inventory_ui)
	# Hide the template after packing; spawned instances must remain visible.
	player_template.hide()
	player_template.set_physics_process(false)

func _process(_delta: float) -> void:
	status_bar.text = network.status_text()

func _on_peer_joined(peer_id: int) -> void:
	pass

func _on_peer_left(peer_id: int) -> void:
	pass

func _on_local_player_ready(_peer_id: int) -> void:
	pass
