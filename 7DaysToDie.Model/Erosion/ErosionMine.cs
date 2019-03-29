using System;
using _7DaysToDie.Model;
using _7DaysToDie.Model.Erosion;
using _7DaysToDie.Model.Model;

namespace _7DaysToDie.Erosion
{
    public class ErosionMine
    {
        private readonly HeightMap2 _heightMap;
        private const int HMAP_SIZE = 4096;

        public ErosionMine(HeightMap2 heightMap)
        {
            _heightMap = heightMap;
        }

        public ushort HMAP(int x, int y)
        {
            return _heightMap[x, y];            
        }
        /*
        public void DEPOSIT_AT(int X, int Z, int W, float ds, Point2[] erosion) 
        { 
            float delta = ds * (W); 
            erosion[HMAP_INDEX((X), (Z))].y+=delta; 
            hmap[HMAP_INDEX((X), (Z))]  +=delta; 
                params.deposit(scolor, surface[HMAP_INDEX((X), (Z))], delta); 
        }

        public void DEPOSIT(int xi, int zi, int xf, int zf, float ds, out ushort H)
        {
            DEPOSIT_AT(xi, zi, (1 - xf) * (1 - zf));
            DEPOSIT_AT(xi + 1, zi, xf * (1 - zf));
            DEPOSIT_AT(xi, zi + 1, (1 - xf) * zf);
            DEPOSIT_AT(xi + 1, zi + 1, xf * zf);
                (H) += ds;
        }

        public void Erode(int iterations, ErosionParameters erosionParameters)
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            
            float Kq = erosionParameters.Kq, 
                Kw = erosionParameters.Kw, 
                Kr = erosionParameters.Kr, 
                Kd = erosionParameters.Kd, 
                Ki = erosionParameters.Ki,
                minSlope = erosionParameters.MinSlope, 
                Kg = erosionParameters.G * 2;

            //TempData < Point2[HMAP_SIZE * HMAP_SIZE] > erosion;
            var erosion = new Point2[HMAP_SIZE * HMAP_SIZE];
            float flt = 0;            
            ushort MAX_PATH_LEN = HMAP_SIZE * 4;
            
            //Int64 t0 =  get_ref_time();
            ulong longPaths = 0, randomDirs = 0, sumLen = 0;
            
            for (ushort iter = 0; iter < iterations; ++iter)
            {                
                // I think these are the randomised droplet location
                var xi = rnd.Next(0, HMAP_SIZE);
                var zi = rnd.Next(0, HMAP_SIZE);

                float xp = xi, zp = zi;

                float xf = 0, zf = 0;

                float h = HMAP(xi, zi);

                float s = 0, v = 0, w = 1;
                vec4f scolor = zero4f();

                float h00 = h;
                float h10 = HMAP(xi + 1, zi);
                float h01 = HMAP(xi, zi + 1);
                float h11 = HMAP(xi + 1, zi + 1);

                float dx = 0, dz = 0;

                ushort numMoves = 0;
                for (; numMoves < MAX_PATH_LEN; ++numMoves)
                {
                    // calc gradient
                    float gx = h00 + h01 - h10 - h11;
                    float gz = h00 + h10 - h01 - h11;
                    //== better interpolated gradient?

                    // calc next pos
                    dx = (dx - gx) * Ki + gx;
                    dz = (dz - gz) * Ki + gz;

                    float dl = sqrtf(dx * dx + dz * dz);
                    if (dl <= FLT_EPSILON)
                    {
                        // pick random dir
                        float a = frnd() * TWOPI;
                        dx = cosf(a);
                        dz = sinf(a);
                        ++randomDirs;
                    }
                    else
                    {
                        dx /= dl;
                        dz /= dl;
                    }

                    float nxp = xp + dx;
                    float nzp = zp + dz;

                    // sample next height
                    int nxi = intfloorf(nxp);
                    int nzi = intfloorf(nzp);
                    float nxf = nxp - nxi;
                    float nzf = nzp - nzi;

                    float nh00 = HMAP(nxi, nzi);
                    float nh10 = HMAP(nxi + 1, nzi);
                    float nh01 = HMAP(nxi, nzi + 1);
                    float nh11 = HMAP(nxi + 1, nzi + 1);

                    float nh = (nh00 * (1 - nxf) + nh10 * nxf) * (1 - nzf) + (nh01 * (1 - nxf) + nh11 * nxf) * nzf;

                    // if higher than current, try to deposit sediment up to neighbour height
                    if (nh >= h)
                    {
                        float ds = (nh - h) + 0.001f;

                        if (ds >= s)
                        {
                            // deposit all sediment and stop
                            ds = s;
                            DEPOSIT(h)
                          s = 0;
                            break;
                        }

                        DEPOSIT(h)
                      s -= ds;
                        v = 0;
                    }

                    // compute transport capacity
                    float dh = h - nh;
                    float slope = dh;
                    //float slope=dh/sqrtf(dh*dh+1);

                    float q = maxval(slope, minSlope) * v * w * Kq;

                    // deposit/erode (don't erode more than dh)
                    float ds = s - q;
                    if (ds >= 0)
                    {
                        // deposit
                        ds *= Kd;
                        //ds=minval(ds, 1.0f);

                        DEPOSIT(dh)
                      s -= ds;
                    }
                    else
                    {
                        // erode
                        ds *= -Kr;
                        ds = minval(ds, dh * 0.99f);

#define ERODE(X, Z, W) \
                        { \
          float delta = ds * (W); \
          hmap[HMAP_INDEX((X), (Z))] -= delta; \
          Point2 & e = erosion[HMAP_INDEX((X), (Z))]; \
          float r = e.x, d = e.y; \
          if (delta <= d) d -= delta; \
          else { r += delta - d; d = 0; } \
          e.x = r; e.y = d; \
          scolor =params.erode(scolor, surface[HMAP_INDEX((X), (Z))], s, delta); \
        }

#if 1
          for (int z=zi-1; z<=zi+2; ++z)
          {
            float zo=z-zp;
            float zo2=zo*zo;

            for (int x=xi-1; x<=xi+2; ++x)
            {
              float xo=x-xp;

              float w=1-(xo*xo+zo2)*0.25f;
              if (w<=0) continue;
              w*=0.1591549430918953f;

              ERODE(x, z, w)
            }
          }
#else
                        ERODE(xi, zi, (1 - xf) * (1 - zf))
                        ERODE(xi + 1, zi, xf * (1 - zf))
                        ERODE(xi, zi + 1, (1 - xf) * zf)
                        ERODE(xi + 1, zi + 1, xf * zf)
                      #endif

                        dh -= ds;

#undef ERODE

                        s += ds;
                    }

                    // move to the neighbour
                    v = sqrtf(v * v + Kg * dh);
                    w *= 1 - Kw;

                    xp = nxp; zp = nzp;
                    xi = nxi; zi = nzi;
                    xf = nxf; zf = nzf;

                    h = nh;
                    h00 = nh00;
                    h10 = nh10;
                    h01 = nh01;
                    h11 = nh11;
                }

                if (numMoves >= MAX_PATH_LEN)
                {
                    debug("droplet #%d path is too long!", iter);
                    ++longPaths;
                }

                sumLen += numMoves;
            }

#undef DEPOSIT
#undef DEPOSIT_AT
            
        }
        
*/
    }
}
