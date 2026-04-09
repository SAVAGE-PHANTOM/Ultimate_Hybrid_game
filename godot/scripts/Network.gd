extends Node

signal peer_joined(peer_id)
signal peer_left(peer_id)
signal local_player_ready(peer_id)

const DEFAULT_PORT := 24570

var player_scene: PackedScene
var world_root: Node
var inventory_ui: Node

var players := {}
var local_peer_id := 0

func init(player_template: Node, world: Node, inv_ui: Node) -> void:
	world_root = world
	inventory_ui = inv_ui

	player_scene = PackedScene.new()
	var ok := player_scene.pack(player_template)
	if not ok:
		push_error("Failed to pack player template into PackedScene")

	_boot_network_from_args()

func _boot_network_from_args() -> void:
	var args := OS.get_cmdline_args()
	var mode := "solo"
	var connect_ip := "127.0.0.1"
	var port := DEFAULT_PORT

	for arg in args:
		if arg == "--server":
			mode = "server"
		elif arg.begins_with("--connect="):
			mode = "client"
			connect_ip = arg.split("=", false, 1)[1]
		elif arg.begins_with("--port="):
			port = int(arg.split("=", false, 1)[1])

	if mode == "server":
		var peer := NetworkedMultiplayerENet.new()
		var err := peer.create_server(port, 16)
		if err != OK:
			push_error("Server create failed: %s" % err)
			_start_solo()
			return
		get_tree().network_peer = peer
		_bind_tree_signals()
		_start_local_player()
	elif mode == "client":
		var peer := NetworkedMultiplayerENet.new()
		var err := peer.create_client(connect_ip, port)
		if err != OK:
			push_error("Client create failed: %s" % err)
			_start_solo()
			return
		get_tree().network_peer = peer
		_bind_tree_signals()
	else:
		_start_solo()

func _bind_tree_signals() -> void:
	get_tree().connect("network_peer_connected", self, "_on_peer_connected")
	get_tree().connect("network_peer_disconnected", self, "_on_peer_disconnected")
	get_tree().connect("connected_to_server", self, "_on_connected_to_server")
	get_tree().connect("connection_failed", self, "_on_connection_failed")
	get_tree().connect("server_disconnected", self, "_on_server_disconnected")

func _start_solo() -> void:
	get_tree().network_peer = null
	_start_local_player()

func _start_local_player() -> void:
	local_peer_id = 1
	if get_tree().network_peer != null:
		local_peer_id = get_tree().get_network_unique_id()
	_spawn_player(local_peer_id, true)
	emit_signal("local_player_ready", local_peer_id)

func _on_connected_to_server() -> void:
	local_peer_id = get_tree().get_network_unique_id()
	_spawn_player(local_peer_id, true)
	emit_signal("local_player_ready", local_peer_id)

func _on_connection_failed() -> void:
	push_error("Connection failed; falling back to solo")
	_start_solo()

func _on_server_disconnected() -> void:
	push_error("Server disconnected; falling back to solo")
	_start_solo()

func _on_peer_connected(id: int) -> void:
	emit_signal("peer_joined", id)
	# Server spawns other peers as they connect. Clients will be told via rpc.
	if get_tree().is_network_server():
		rpc("rpc_spawn_player", id)

func _on_peer_disconnected(id: int) -> void:
	emit_signal("peer_left", id)
	if players.has(id):
		players[id].queue_free()
		players.erase(id)

remote func rpc_spawn_player(id: int) -> void:
	if id == local_peer_id:
		return
	if players.has(id):
		return
	_spawn_player(id, false)

func _spawn_player(peer_id: int, is_local: bool) -> void:
	var p := player_scene.instance()
	p.name = "Player_%d" % peer_id
	p.set("peer_id", peer_id)
	p.set("is_local", is_local)
	p.translation = Vector3(rand_range(-8, 8), 1, rand_range(-8, 8))
	p.show()
	p.set_physics_process(true)
	world_root.add_child(p)
	players[peer_id] = p
	if is_local:
		p.connect("inventory_changed", inventory_ui, "_on_inventory_changed")
		inventory_ui.call("bind_player", p)

func status_text() -> String:
	var mode := get_tree().network_peer == null ? "SOLO" : (get_tree().is_network_server() ? "HOST" : "CLIENT")
	return "%s | Players %d | Args: --server or --connect=IP --port=%d" % [mode, players.size(), DEFAULT_PORT]
