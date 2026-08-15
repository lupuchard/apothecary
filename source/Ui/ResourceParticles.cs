using System;
using Godot;
using System.Collections.Generic;

namespace Apothecary;

public partial class ResourceParticles : Node2D {
	private const int PARTICLE_COUNT = 64;

	private GpuParticles2D? model;
	//private ParticleProcessMaterial? particle_material;
	private readonly List<GpuParticles2D> particle_nodes = [];
	private readonly List<int> unused_particles = [];
	private readonly List<Texture2D> resource_textures = [];

	public override void _Ready() {
		model = GetChild<GpuParticles2D>(0);
		model.Hide();
		
		Game.Instance.ResourceUpdated += OnAcquireResource;
		/*particle_material = new ParticleProcessMaterial();
		particle_material.ParticleFlagDisableZ = true;
		particle_material.Direction = new Vector3(0.0f, -1.0f, 0.0f);
		particle_material.Spread = 135.0f;
		particle_material.Gravity = new Vector3(0.0f, 140.0f, 0.0f);
		particle_material.InitialVelocityMin = 100.0f;
		particle_material.InitialVelocityMax = 100.0f;*/

		for (var i = 1; i < (int)Resource.COUNT; i++) {
			resource_textures.Add(GD.Load<Texture2D>(((Resource)i).SmallSpritePath()));
		}
	}

	public void OnAcquireResource(Resource resource, int amount) {
		var particles = GetParticles();
		particles.Texture = resource_textures[(int)resource];
		particles.AmountRatio = Math.Min(amount, 64) / 64.0f;
		particles.Emitting = true;
		particles.GlobalPosition = GetViewport().GetMousePosition();
		particles.Modulate = resource.GetColor();
	}

	private GpuParticles2D GetParticles() {
		if (unused_particles.Count > 0) {
			var particles = particle_nodes[unused_particles[-1]];
			unused_particles.RemoveAt(unused_particles.Count - 1);
			return particles;
		} else {
			/*var new_particles = new GpuParticles2D();
			new_particles.Amount = PARTICLE_COUNT;
			new_particles.Lifetime = 3.0;
			new_particles.Explosiveness = 1.0f;
			new_particles.OneShot = true;
			new_particles.ProcessMaterial = particle_material;*/
			var new_particles = (GpuParticles2D)model!.Duplicate();
			new_particles.Amount = PARTICLE_COUNT;
			new_particles.OneShot = true;
			var idx = particle_nodes.Count;
			new_particles.Finished += () => {
				unused_particles.Add(idx);
			};
			AddChild(new_particles);
			particle_nodes.Add(new_particles);
			return new_particles;
		}
	}
}
