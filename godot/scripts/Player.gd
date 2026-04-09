extends KinematicBody2D

signal elimination
signal request_interact
signal request_scan
signal request_uav

var health := 100.0
var stamina := 100.0
var max_stamina := 100.0

var base_speed := 160.0
var sprint_mult := 1.5
var walk_mult := 0.55
var ads_mult := 0.75
var crouch_mult := 0.78
var prone_mult := 0.55

var is_crouched := false
var is_prone := false
var is_ads := false
var hip_aim := false

var ammo_in_mag := 30
var reserve_ammo := 90
var eliminations := 0

var is_automatic := true
var fire_interval := 0.11
var fire_timer := 0.0

var in_vehicle := false
var vehicle_ref := null

func _physics_process(delta: float) -> void:
	if fire_timer > 0.0:
		fire_timer = max(0.0, fire_timer - delta)

	if Input.is_action_just_pressed("hip_aim_toggle"):
		hip_aim = !hip_aim

	if Input.is_action_just_pressed("crouch"):
		is_crouched = !is_crouched
		if is_crouched:
			is_prone = false

	if Input.is_action_just_pressed("prone"):
		is_prone = !is_prone
		if is_prone:
			is_crouched = false

	is_ads = Input.is_action_pressed("ads")

	if Input.is_action_just_pressed("toggle_fire_mode"):
		is_automatic = !is_automatic
		fire_interval = is_automatic ? 0.11 : 0.24

	if Input.is_action_just_pressed("scorestreak_uav"):
		emit_signal("request_uav")

	if Input.is_action_just_pressed("quick_marker"):
		emit_signal("request_scan")

	if Input.is_action_just_pressed("interact"):
		emit_signal("request_interact")

	if in_vehicle and vehicle_ref != null:
		vehicle_ref.drive_from_input(delta)
		position = vehicle_ref.position
		return

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

	var speed := base_speed
	if is_prone:
		speed *= prone_mult
	elif is_crouched:
		speed *= crouch_mult
	if is_ads:
		speed *= ads_mult

	if Input.is_action_pressed("walk"):
		speed *= walk_mult

	var sprinting := Input.is_action_pressed("sprint") and not is_ads and dir.length() > 0.0 and stamina > 0.0
	if sprinting:
		speed *= sprint_mult
		stamina = max(0.0, stamina - 24.0 * delta)
	else:
		stamina = min(max_stamina, stamina + 18.0 * delta)

	move_and_slide(dir * speed)

	if Input.is_action_pressed("fire"):
		_try_fire()

func _try_fire() -> void:
	if ammo_in_mag <= 0:
		return
	if fire_timer > 0.0:
		return
	fire_timer = fire_interval
	ammo_in_mag -= 1

func pickup(item_id: String) -> void:
	if item_id == "ammo_556":
		reserve_ammo += 30
	elif item_id == "medkit":
		health = min(100.0, health + 40.0)
	elif item_id == "boost":
		stamina = min(max_stamina, stamina + 35.0)

func enter_vehicle(vehicle) -> void:
	in_vehicle = true
	vehicle_ref = vehicle
	vehicle_ref.occupied = true

func exit_vehicle(vehicle) -> void:
	in_vehicle = false
	vehicle_ref = null
	vehicle.occupied = false
	position = vehicle.position + Vector2(-44, 0)

