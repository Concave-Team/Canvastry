local transform

function Start()
	transform = GameObject.GetComponent("TransformComponent")
end

function Update(dt)
	if Input.IsKeyDown(Key.W) then
		transform.Position = transform.Position + Vector2.__new(0,-5 * dt * 16)
	elseif Input.IsKeyDown(Key.A) then
		transform.Position = transform.Position + Vector2.__new(-5 * dt * 16,0)
	elseif Input.IsKeyDown(Key.S) then
		transform.Position = transform.Position + Vector2.__new(0,5 * dt * 16)
	elseif Input.IsKeyDown(Key.D) then
		transform.Position = transform.Position + Vector2.__new(5 * dt * 16,0)
	end
end

function OnCollisionEnter(collider)
	transform.Position = transform.Position + Vector2.__new(3,3)
end

function OnCollisionExited()

end