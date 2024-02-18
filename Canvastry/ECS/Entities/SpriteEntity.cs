using Canvastry.ECS.Components;
using Canvastry.Internals.Assets;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canvastry.ECS.Entities
{
    public class SpriteEntity : Entity, IDrawableEntity
    {
        public SpriteEntity() : base()
        {
            this.AddComponent<TransformComponent>(new TransformComponent());
            this.AddComponent<SpriteComponent>(new SpriteComponent());
        }

        public override void Init()
        {
            base.Init();
        }

        public override void Update()
        {
            base.Update();
        }

        public void Draw()
        {
            var spriteComp = this.GetComponent<SpriteComponent>();
            var transformComp = this.GetComponent<TransformComponent>();
            if ((spriteComp.Texture as LazyAsset).asset != null)
            {
                var texture = ((spriteComp.Texture as LazyAsset).asset.Data as TextureAssetRef).Texture;

                Raylib.DrawTexturePro(
                        texture,
                        new Rectangle(0, 0, texture.Width, texture.Height),
                        new Rectangle(transformComp.Position, transformComp.Size * transformComp.Scale),
                        transformComp.Size / 2f,
                        transformComp.Rotation,
                        spriteComp.Tint
                    );
            }

            if(HasComponent<BoxColliderComponent>())
            {
                var bc = GetComponent<BoxColliderComponent>();

                Raylib.DrawRectangleLines((int)bc.Position.X, (int)bc.Position.Y, (int)bc.Size.X, (int)bc.Size.Y, Color.Yellow);
            }
        }


    }
}
