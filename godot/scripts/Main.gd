extends Node2D

const EnemyScene := preload("res://scripts/Enemy.gd")
const LootScene := preload("res://scripts/Loot.gd")

onready var world := $World
onready var player := $World/Player
onready var vehicle := $World/Vehicle
onready var status := $HUD/Status

var enemies := []
var loot := []

var zone_center := Vector2(640, 360)
var zone_radius := 520.0
var zone_shrink_rate := 5.5
var zone_min_radius := 120.0
var zone_damage_per_second := 8.0

var extraction_point := Vector2(1120, 640)
var extraction_radius := 44.0
var extraction_unlocked := false
var extraction_progress := 0.0
var extraction_goal := 2.0

var inventory_open := false

var score := 0
var streak := 0
var uav_active := false
var uav_timer := 0.0
var uav_duration := 8.0

func _ready() -> void:
	_spawn_enemies()
	_spawn_loot()
	player.connect("elimination", self, "_on_elimination")
	player.connect("request_interact", self, "_on_player_interact")
	player.connect("request_scan", self, "_on_player_scan")
	player.connect("request_uav", self, "_on_player_uav")
	status.text = _status_text()

func _process(delta: float) -> void:
	if Input.is_action_just_pressed("inventory"):
		inventory_open = !inventory_open

	if inventory_open:
		status.text = "INVENTORY (prototype): press Tab to close"
		return

	_update_uav(delta)
	_update_zone(delta)
	_update_extraction(delta)
	_update_enemies(delta)
	_apply_zone_damage(delta)
	status.text = _status_text()

func _status_text() -> String:
	var uav_text := ""
	if score >= 400 and not uav_active:
		uav_text = "UAV READY (G)"
	elif uav_active:
		uav_text = "UAV %.1fs" % uav_timer
	else:
		uav_text = "Score %d Streak %d" % [score, streak]

	var objective := extraction_unlocked ? "Extract: hold F in beacon" : "Unlock extraction: eliminations %d/4" % player.eliminations
	return "HP %d  STM %d  Ammo %d/%d  %s  Zone %d  %s" % [
		int(player.health),
		int(player.stamina),
		player.ammo_in_mag,
		player.reserve_ammo,
		uav_text,
		int(zone_radius),
		objective
	]

func _spawn_enemies() -> void:
	var points := [
		Vector2(340, 180), Vector2(520, 260), Vector2(720, 180),
		Vector2(860, 280), Vector2(1020, 460), Vector2(760, 560),
		Vector2(460, 540)
	]
	for i in range(points.size()):
		var e := EnemyScene.new()
		e.position = points[i]
		e.name = "Raider%d" % (i + 1)
		world.add_child(e)
		enemies.append(e)

func _spawn_loot() -> void:
	var points := [Vector2(220, 500), Vector2(600, 420), Vector2(920, 180)]
	for i in range(points.size()):
		var l := LootScene.new()
		l.position = points[i]
		l.item_id = i == 0 ? "medkit" : (i == 1 ? "ammo_556" : "boost")
		world.add_child(l)
		loot.append(l)

func _update_zone(delta: float) -> void:
	if zone_radius > zone_min_radius:
		zone_radius = max(zone_min_radius, zone_radius - zone_shrink_rate * delta)

func _apply_zone_damage(delta: float) -> void:
	if player.position.distance_to(zone_center) > zone_radius:
		player.health -= zone_damage_per_second * delta

func _update_enemies(delta: float) -> void:
	for e in enemies:
		if not e.alive:
			continue
		e.update_ai(delta, player.position, uav_active)

	if not extraction_unlocked and player.eliminations >= 4:
		extraction_unlocked = true

func _update_extraction(delta: float) -> void:
	if not extraction_unlocked:
		extraction_progress = 0.0
		return
	var in_beacon := player.position.distance_to(extraction_point) <= extraction_radius
	if in_beacon and Input.is_action_pressed("interact"):
		extraction_progress += delta
		if extraction_progress >= extraction_goal:
			status.text = "EXTRACTION COMPLETE (prototype)"
	else:
		extraction_progress = max(0.0, extraction_progress - delta * 1.2)

func _update_uav(delta: float) -> void:
	if not uav_active:
		return
	uav_timer -= delta
	if uav_timer <= 0.0:
		uav_timer = 0.0
		uav_active = false

func _on_elimination() -> void:
	score += 100
	streak += 1

func _on_player_scan() -> void:
	for e in enemies:
		if e.alive and e.position.distance_to(player.position) <= 250.0:
			e.reveal_timer = 6.0

func _on_player_uav() -> void:
	if score < 400 or uav_active:
		return
	score -= 400
	uav_active = true
	uav_timer = uav_duration

func _on_player_interact() -> void:
	# pickup loot or enter/exit vehicle
	for l in loot:
		if l == null:
			continue
		if l.position.distance_to(player.position) <= 34.0:
			player.pickup(l.item_id)
			loot.erase(l)
			l.queue_free()
			return

	if vehicle.position.distance_to(player.position) <= 52.0:
		if player.in_vehicle:
			player.exit_vehicle(vehicle)
		else:
			player.enter_vehicle(vehicle)

