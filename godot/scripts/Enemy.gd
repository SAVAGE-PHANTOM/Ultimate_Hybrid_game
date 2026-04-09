extends Node2D

var health := 100.0
var alive := true
var reveal_timer := 0.0
var attack_cd := 0.0

func _ready() -> void:
	_add_sprite()

func _process(delta: float) -> void:
	if reveal_timer > 0.0:
		reveal_timer = max(0.0, reveal_timer - delta)
	if attack_cd > 0.0:
		attack_cd = max(0.0, attack_cd - delta)
	if alive and health <= 0.0:
		alive = false

func update_ai(delta: float, player_pos: Vector2, uav_active: bool) -> void:
	if not alive:
		return
	var to_p := player_pos - position
	var dist := to_p.length()
	var dir := dist > 0.01 ? to_p / dist : Vector2.ZERO
	var speed := (reveal_timer > 0.0 or uav_active) ? 88.0 : 106.0
	if dist > 110.0:
		position += dir * speed * delta

func _add_sprite() -> void:
	var rect := ColorRect.new()
	rect.color = Color(0.35, 0.88, 0.38, 1)
	rect.rect_size = Vector2(22, 22)
	rect.rect_position = Vector2(-11, -11)
	add_child(rect)

