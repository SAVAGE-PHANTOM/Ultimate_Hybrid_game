extends KinematicBody

signal inventory_changed(inventory_text)

export var peer_id := 0
export var is_local := false

var health := 100.0
var stamina := 100.0
var max_stamina := 100.0

var base_speed := 6.0
var sprint_mult := 1.55
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

var is_automatic := true
var fire_interval := 0.11
var fire_timer := 0.0

var velocity := Vector3.ZERO
var gravity := -24.0
var jump_impulse := 9.5

var inventory := {}

onready var camera_rig := $CameraRig
onready var spring_arm := $CameraRig/SpringArm
onready var cam := $CameraRig/SpringArm/Camera

func _ready() -> void:
	set_physics_process(true)
	if not is_local:
		# Remote players don't own the camera.
		cam.current = false

func _physics_process(delta: float) -> void:
	if fire_timer > 0.0:
		fire_timer = max(0.0, fire_timer - delta)

	if is_local:
		_handle_local_input(delta)
	else:
		# Remote updates via RPC.
		pass

	velocity.y += gravity * delta
	velocity = move_and_slide(velocity, Vector3.UP)

	if is_local:
		rpc_unreliable("rpc_sync_transform", global_transform)

func _handle_local_input(delta: float) -> void:
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

	var dir2 := Vector2.ZERO
	if Input.is_action_pressed("move_forward"):
		dir2.y -= 1
	if Input.is_action_pressed("move_back"):
		dir2.y += 1
	if Input.is_action_pressed("move_left"):
		dir2.x -= 1
	if Input.is_action_pressed("move_right"):
		dir2.x += 1
	dir2 = dir2.normalized()

	var forward := -global_transform.basis.z
	var right := global_transform.basis.x
	var move_dir := (forward * dir2.y + right * dir2.x)
	move_dir.y = 0
	move_dir = move_dir.normalized()

	var speed := base_speed
	if is_prone:
		speed *= prone_mult
	elif is_crouched:
		speed *= crouch_mult
	if is_ads:
		speed *= ads_mult
	if Input.is_action_pressed("walk"):
		speed *= walk_mult

	var sprinting := Input.is_action_pressed("sprint") and not is_ads and dir2.length() > 0.0 and stamina > 0.0
	if sprinting:
		speed *= sprint_mult
		stamina = max(0.0, stamina - 24.0 * delta)
	else:
		stamina = min(max_stamina, stamina + 18.0 * delta)

	velocity.x = move_dir.x * speed
	velocity.z = move_dir.z * speed

	if Input.is_action_just_pressed("jump") and is_on_floor() and not is_prone:
		velocity.y = jump_impulse

	if Input.is_action_pressed("fire"):
		_try_fire()

	_update_camera(delta)

func _update_camera(delta: float) -> void:
	# Simple third-person orbit: mouse movement not wired to keep Catalina simple.
	var yaw := 0.0
	if Input.is_action_pressed("lean_left"):
		yaw = 1.0
	elif Input.is_action_pressed("lean_right"):
		yaw = -1.0
	if yaw != 0.0:
		rotate_y(yaw * delta * 1.8)

	if Input.is_action_pressed("ads"):
		spring_arm.spring_length = lerp(spring_arm.spring_length, 3.1, delta * 10.0)
		cam.fov = lerp(cam.fov, 55.0, delta * 10.0)
	else:
		spring_arm.spring_length = lerp(spring_arm.spring_length, 6.5, delta * 10.0)
		cam.fov = lerp(cam.fov, 70.0, delta * 10.0)

func _try_fire() -> void:
	if ammo_in_mag <= 0:
		return
	if fire_timer > 0.0:
		return
	fire_timer = fire_interval
	ammo_in_mag -= 1

func add_item(item_id: String, amount: int) -> void:
	if not inventory.has(item_id):
		inventory[item_id] = 0
	inventory[item_id] += amount
	emit_signal("inventory_changed", inventory_text())

func inventory_text() -> String:
	var lines := PoolStringArray()
	for k in inventory.keys():
		lines.append("%s x%d" % [str(k), int(inventory[k])])
	return lines.join("\n")

remote func rpc_sync_transform(t: Transform) -> void:
	if is_local:
		return
	global_transform = t

