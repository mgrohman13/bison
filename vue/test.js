let app = {};


app.random = {
  counter: 0xAD17FE14704E2,

  mutate: function () {
    this.counter = (this.counter + 0x073B7B6E1CA03) % 0x10000000000000;
    // console.log(this.counter.toString(16).padStart(13, '0'));
    let t = this.oeInt(996) + 4;
    console.log(t);
    setTimeout(this.mutate.bind(this), t);
    // console.log(this.counter.toString(16).padStart(13, '0'));
  },

  random: function () {
    const bits = 26;
    const mask = 0x3FFFFFF
    const mult = 0x4000000;

    function log(val) {
      // console.log(val.toString(2).padStart(32, '0').match(/.{1,4}/g).join('-'));
    };
    function shift(value) {
      log(value);
      const max = bits - 1;
      let shift = value % (max * 4);
      let neg = (shift / max) >>> 0;
      shift = (shift % max) + 1;
      // console.log(neg);
      // console.log(shift);
      let v1 = ((((neg & 1) === 1 ? value : ~value) & mask) << shift) >>> 0;
      let v2 = ((((neg & 2) === 2 ? value : ~value) & mask)) >>> (bits - shift);
      log(v1);
      log(v2);
      value = ((value ^ (v1 | v2)) & mask) >>> 0;
      log(value);
      return value;
    };

    let time = new Date().getTime();
    let timeUp = (time / mult) >>> 0;
    // console.log(timeUp);
    let timeLow = (time & mask) >>> 0;
    let counterUp = (this.counter / mult) >>> 0;
    let counterLow = (this.counter & mask) >>> 0;
    log(timeLow);
    log(counterLow);
    let upper = shift(timeLow + counterLow);
    // console.log('\n');
    log(timeUp);
    log(counterUp);
    let lower = shift(timeUp + counterUp);

    const fullMult = 0x10000000000000;
    let random = Math.random();
    let randomUp = ((random * 0x100000000)) >>> (32 - bits);
    let randomLow = ((random * fullMult) & mask) >>> 0;
    // console.log('\n');
    log(randomUp);
    log(upper);
    randomUp = ((randomUp + upper) & mask) >>> 0;
    log(randomUp);
    // console.log('\n');
    log(randomLow);
    log(lower);
    randomLow = ((randomLow + lower) & mask) >>> 0;
    log(randomLow);
    let result = ((randomUp * mult) + (randomLow & mask));
    this.counter = (result + 0xB91DB057A1873) % fullMult;
    result /= fullMult;
    // console.log('\n');
    // console.log(this.counter.toString(16).padStart(13, '0'));
    // console.log(result);
    return result;
  },

  float: function (max = 1) {
    return this.random() * max;
  },

  next: function (max) {
    if (Math.floor(max) !== max)
      throw new Error(`random.next ${max}`);

    return this.rangeInt(0, max - 1);
  },

  round: function (avg) {
    let result = Math.floor(avg);
    avg -= result;
    if (this.bool(avg))
      result++;
    return result;
  },

  bool(chance = .5) {
    if (chance < 0 || chance > 1)
      throw new Error(`random.chance ${chance}`);

    return (this.float() < chance);
  },

  rangeInt: function (min, max) {
    if (Math.floor(min) !== min || Math.floor(max) !== max)
      throw new Error(`random.rangeInt ${min} ${max}`);

    if (min > max) {
      [min, max] = [max, min];
    }
    return Math.floor(min + this.float(max - min + 1));
  },

  range: function (min, max) {
    if (min > max) {
      [min, max] = [max, min];
    }
    return min + this.float(max - min);
  },

  oeInt: function (avg = 1) {
    return Math.floor(-this.oe() / Math.log(avg / (avg + 1)));
  },

  oe: function (avg = 1) {
    return avg * -Math.log(1 - this.float(1));
  },

  gaussianInt: function (average, dev, cap) {
    return this.gaussian(average, dev, cap, true);
  },

  gaussian: function (average, dev, cap, int = false) {
    if (average === cap)
      return average;

    let a, b, c = 0;
    while (c > 1 || c === 0) {
      a = this.float() * 2 - 1;
      b = this.float() * 2 - 1;
      c = a * a + b * b;
    }
    let result = average + average * dev * a * Math.sqrt((-2 * Math.log(c)) / c);

    if (!isNaN(cap)) {
      if (cap > average)
        cap = average * 2 - cap;
      if (int)
        cap--;

      if (result < cap)
        result = average - ((average - result) % (average - cap));
      else if (result > (average * 2 - cap))
        result = average + ((result - average) % (average - cap));

      if (int) {
        if (Math.floor(cap) !== cap)
          throw new Error(`random.gaussian cap ${cap}`);

        if (average - cap <= 1.5)
          return this.round(average);

        cap++;
        if (result > average) {
          let upperDbl = average * 2 - cap;
          let upperCap = Math.ceil(upperDbl);
          let diff = upperCap - upperDbl;
          result = this.round(result + diff);
          if (result <= upperCap)
            result = this.round(result - diff)
          else
            result = this.gaussian(average, dev, cap, int);
        }
        else {
          result = this.round(result);
          if (result < cap)
            result = this.gaussian(average, dev, cap, int);
        }
      }
    } else if (int) {
      result = this.round(result);
    }

    return result;
  },

  gaussianOEInt: function (average, dev, oe, cap) {
    let a = this.splitAvg(average, oe, cap);
    if (!isNaN(a[2]))
      a[2] = Math.ceil(a[2]);
    return this.gaussianInt(a[0], dev, a[2]) + this.oeInt(a[1]);
  },

  gaussianOE: function (average, dev, oe, cap) {
    let a = this.splitAvg(average, oe, cap);
    return this.gaussian(a[0], dev, a[2]) + this.oe(a[1]);
  },

  splitAvg: function (average, oe, cap) {
    if (!isNaN(cap) && cap > average)
      cap = average * 2 - cap;
    oe *= average;
    average -= oe;
    if (!isNaN(cap) && cap > average) {
      oe += average - cap;
      average = cap;
      if (oe < 0)
        throw new Error(`random.splitAvg ${average} ${oe} ${cap}`);
    }
    return [average, oe, cap];
  },

  shuffle: function (array) {
    for (let a = array.length; a > 1;) {
      let b = this.next(a--);
      if (a !== b)
        [array[a], array[b]] = [array[b], array[a]];
    }
    return array;
  },

};
app.random.mutate();
// Math.random = app.random.float;


// let serialize = function (o) {
//   let j = {};
//   let p = Object.getPrototypeOf(o);
//   for (let k of Object.getOwnPropertyNames(p)) {
//     const desc = Object.getOwnPropertyDescriptor(p, k);
//     const hasGetter = desc && typeof desc.get === 'function';
//     if (hasGetter) {
//       j[k] = o[k];
//     }
//   }
//   return j;
// }


app.consts = {
  dimension: function (mult = 1) {
    return app.random.gaussianOEInt(13 * mult, .091, .078, 5);
  },
  heightMult: .65,
  startDev: 1.3,
  startUnitDev: .65,
  startUnits: 6,

  cityCost: function (tile) {
    let cities = tile.player.count(v => v instanceof City);
    let cost = app.random.gaussianOEInt(1.3 + Math.pow(tile.size + 2.1, .78) * Math.pow(cities / 5.2, 1.69), .169, .065, 1);
    let cur = tile.unmoved.length;
    if (cur < cost)
      if (app.random.bool(cur * cur / cost / cost))
        cost = cur;
      else
        cost = -1;
    return cost;
  },
  cityInc: function (size, units) {
    let avg = Math.pow(size + .13, .39) / Math.sqrt(units / 52 + 1.3) / 1.69;
    return app.random.gaussianOEInt(avg, .13, .13, 0);
  },
  enemyUnitDiv: 2.6,

  tileStarChance: 1 / 13,
  tileSize: function () {
    return app.random.gaussianOEInt(6.5, .52, .26, 1);
  },
};
Object.freeze(app.consts);


class Game {
  constructor() {

    // while (true) {
    //   let v1 = app.random.next(0x10000000000000);
    //   let v2 = app.random.next(0x10000000000000);
    //   let v3 = app.random.next(0x10000000000000);
    //   let b1 = v1.toString(2).padStart(52, '0');
    //   let b2 = v2.toString(2).padStart(52, '0');
    //   let b3 = v3.toString(2).padStart(52, '0');

    //   let add = (v2 + v3) % 0x10000000000000;
    //   let addB = add.toString(2).padStart(52, '0');

    //   if ((b1.match(/0/g) || []).length == 26 && (b2.match(/0/g) || []).length == 26 && (b3.match(/0/g) || []).length == 26 && (addB.match(/0/g) || []).length == 26) {
    //     console.log((b1.match(/1/g) || []).length);
    //     console.log(v1.toString(16).padStart(13, '0'));
    //     console.log((b2.match(/1/g) || []).length);
    //     console.log(v2.toString(16).padStart(13, '0'));
    //     console.log((b3.match(/1/g) || []).length);
    //     console.log(v3.toString(16).padStart(13, '0'));
    //     // console.log(val);
    //     // console.log(binary);


    //     break;
    //   }
    // }

    this._players = [];
    this._players.push(new Player(this, 'player', '#0000FF'));
    this._players.push(new Player(this, 'enemy ', '#FF0000'));

    this._turn = 0;
    this._width = app.consts.dimension();
    this._height = app.consts.dimension(app.consts.heightMult);
    this._map = [[]];

    for (let x = 0; x < this.width; x++) {
      this._map[x] = [];
      for (let y = 0; y < this.height; y++) {
        this._map[x][y] = new Tile(this, x, y);
      }
    }

    let start;
    do {
      start = this.center(app.consts.startDev);
    } while (start.star);
    new City(this.player, start);
    for (let a = 0; a < app.consts.startUnits; a++) {
      let tile;
      do {
        tile = this.tile(start.x, start.y, app.consts.startUnitDev);
      } while (tile.star || tile.space < 1)
      new Unit(this.player, tile);
    }

    for (let a = 0; a < 4; a++) {
      let enemy = null;
      do {
        let x = (a === 0 ? 0 : a === 1 ? this.width - 1 : app.random.next(this.width));
        let y = (a === 2 ? 0 : a === 3 ? this.height - 1 : app.random.next(this.height));
        enemy = this.map(x, y);
      } while (enemy.star || enemy.player);
      new City(this.players[1], enemy);
    }

    this.end();

    this.select(start);
  };

  get player() { return this.players[0]; };
  get players() { return [...this._players]; };
  get turn() { return this._turn; };
  get width() { return this._width; };
  get height() { return this._height; };

  map = function (x, y) {
    if (x < 0 || x >= this._width || y < 0 || y >= this._height)
      return null;
    return this._map[x][y];
  };
  center = function (dev) {
    let f = function (v) {
      return (v - 1) / 2;
    };
    return this.tile(f(this.width), f(this.height), dev);
  };
  tile = function (x, y, dev) {
    let f = function (v, c) {
      return app.random.gaussianInt(v, dev / v, Math.max(2, 2 * v - c + 3));
    };
    return this.map(f(x, this.width), f(y, this.height));
  };

  get selected() { return this._selected; };
  select(tile) {
    if (this._selected)
      this._selected.selected = false;
    tile.selected = true;
    this._selected = tile;
  };

  end() {
    console.log(this.turn);
    this._turn++;
    this.player.end();

    for (let a = 1; a < this.players.length; a++) {
      this.players[a].play();
      this.players[a].end();
    }
  };
}

class Tile {
  constructor(game, x, y) {
    this._game = game;
    this._selected = false;
    this._x = x;
    this._y = y;

    if (app.random.bool(app.consts.tileStarChance))
      this._size = '*';
    else
      this._size = app.consts.tileSize();

    this._city = null;
    this._units = [];
  };

  get selected() {
    return this._selected;
  };
  set selected(value) { this._selected = value; };

  get game() { return this._game; };
  get x() { return this._x; };
  get y() { return this._y; };

  get star() {
    return this._size === '*';
  };
  get size() {
    if (this.star)
      throw new Error('Tile.size');
    return this._size;
  };
  get space() {
    return this.size - this.count;
  };

  get player() {
    if (this.city)
      return this.city.player;
    if (this.count)
      return this.units[0].player;
  };

  get city() { return this._city; };
  get units() { return [...this._units]; };
  get count() { return this.units.length; };
  get unmoved() {
    return this.units.filter(unit => !unit.moved);
  }

  inc() {
    this._size++;
  };

  build() {
    if (this.player && !this.city && !this.star) {
      let cost = app.consts.cityCost(this);
      if (cost > -1) {
        new City(this.player, this);
        let used = app.random.shuffle(this.units).slice(0, cost);
        used.forEach(u => u.die());
      } else {
        this.markMoved();
      }
    }
  };

  move(tile) {
    if (this.distance(tile) !== 1)
      throw new Error(`Tile.move (${this.x},${this.y}) -> (${tile.x},${tile.y})`);

    if (tile.player && tile.player != this.player) {
      this.attack(tile);
    } else {
      let move = app.random.shuffle(this.unmoved);
      let amt = move.length;
      if (!tile.star)
        amt = Math.min(amt, tile.space);
      if (amt) {
        move = move.slice(0, amt);
        move.forEach(function (unit) {
          this.remove(unit);
          tile.add(unit);
        }.bind(this));
      }
    }
  };

  attack(tile) {
    let r = function (a) {
      return app.random.shuffle(a).map(function (u) {
        return { unit: u, roll: app.random.next(6), };
      }).sort(function (a, b) {
        return a.roll - b.roll;
      });
    };

    let att = r(this.unmoved);
    if (att.length) {
      let def = r(tile.units);
      if (def.length) {
        if (att.length >= def.length)
          att.reverse();
        else
          def.reverse();

        let m = function (a) {
          return a.map(b => b.roll + 1);
        };
        console.log('\n');
        console.log(`${this.player.name} ${m(att)}`);
        console.log(`${tile.player.name} ${m(def)}`);

        for (let a = 0; a < Math.min(att.length, def.length); a++) {
          if (att[a].roll > def[a].roll) {
            def[a].unit.die();
          } else if (def[a].roll > att[a].roll) {
            att[a].unit.die();
          } else {
            def[a].unit.die();
            att[a].unit.die();
          }
        }
      }

      if (tile.count === 0) {
        tile.capture(this.player);
        this.move(tile);
      } else {
        this.markMoved();
      }
    };
  };

  markMoved() {
    this.units.forEach(u => u.moved = true);
  };

  capture(player) {
    if (this.city) {
      this.city.die();
      if (app.random.bool())
        new City(player, this);
    }
  };

  add(piece) {
    if (this.player && this.player !== piece.player)
      throw new Error('Tile.add player');
    if (piece instanceof City) {
      if (this.city)
        throw new Error('Tile.add City');
      this._city = piece;
    } else {
      if (!this.star && this.count >= this.size)
        throw new Error('Tile.add size');
      this._units.push(piece);
    }
    piece.tile = this;
  };
  remove(piece) {
    if (this.player && this.player !== piece.player)
      throw new Error('Tile.add player');
    if (piece instanceof City) {
      if (this.city !== piece)
        throw new Error(`Tile.remove City`);
      this._city = null;
    } else {
      let a = this.units.indexOf(piece);
      if (a < 0)
        throw new Error(`Tile.remove Unit`);
      this._units.splice(a, 1);
    }
  };

  distance(tile) {
    return Math.abs(this.x - tile.x) + Math.abs(this.y - tile.y);
  };
}

class Player {
  constructor(game, name, color) {
    this._game = game;
    this._name = name;
    this._color = color;
    this._pieces = [];
  };

  get game() { return this._game; };
  get name() { return this._name; };
  get color() { return this._color; };
  get pieces() { return [...this._pieces]; };


  add(piece) {
    this._pieces.push(piece);
  };
  remove(piece) {
    let a = this.pieces.indexOf(piece);
    if (a < 0)
      throw new Error(`Player.remove`);
    this._pieces.splice(a, 1);
  };

  count(f) {
    return this.pieces.reduce(function (s, v) {
      return s + (f(v) ? 1 : 0);
    }, 0);
  };

  end() {
    let units = this.count(v => v instanceof Unit);
    if (this !== this.game.player)
      units /= app.consts.enemyUnitDiv;
    this.pieces.forEach(function (piece) {
      piece.end(units);
    });
  };

  play() {
    let moves = [];
    this.pieces.filter(piece => piece instanceof Unit).map(piece => piece.tile)
      .filter(function (value, index, self) {
        return self.indexOf(value) === index;
      }).forEach(function (tile) {
        if (app.random.bool(tile.star ? .5 : tile.count / tile.size)) {
          if (app.random.bool())
            tile.build();
          let targets = this.game.player.pieces.filter(target => target instanceof City);
          // targets.reduce();
          let min = Number.MAX_VALUE;
          targets.forEach(function (target) {
            min = Math.min(min, tile.distance(target.tile));
          });
          targets = targets.filter(target => min === tile.distance(target.tile));
          let target = targets[app.random.next(targets.length)].tile;
          moves.push({ from: tile, to: target });
        }
      }.bind(this));
    app.random.shuffle(moves);
    moves.forEach(function (o) {
      let x = Math.sign(o.to.x - o.from.x);
      let y = Math.sign(o.to.y - o.from.y);
      if (x !== 0 && (y === 0 || app.random.bool()))
        y = 0;
      else
        x = 0;
      x += o.from.x;
      y += o.from.y;
      o.from.move(this.game.map(x, y));
    }.bind(this));
  };
}

class Piece {
  constructor(player, tile) {
    this._player = player;
    this._tile = tile;
    this._moved = false;

    this.player.add(this);
    this.tile.add(this);
  };

  get player() { return this._player; };
  get tile() { return this._tile; };
  set tile(value) {
    this._tile = value;
    this.moved = true;
  };
  get moved() { return this._moved; };
  set moved(value) { this._moved = true; };

  die() {
    this.player.remove(this);
    this.tile.remove(this);
  };

  end() {
    this._moved = false;
  };
}
class City extends Piece {
  constructor(player, tile) {
    super(player, tile);
    if (tile.star)
      throw new Error('City.constructor');
  };

  end(units) {
    super.end();

    let amt = app.consts.cityInc(this.tile.size, units);
    if (amt) {
      if (amt <= this.tile.space)
        for (let a = 0; a < amt; a++)
          new Unit(this.player, this.tile);
      else if (app.random.bool(amt / (amt + Math.pow(this.tile.size, .26) / 3.9)))
        this.tile.inc();
    }
  };
}
class Unit extends Piece {
  constructor(player, tile) {
    super(player, tile);
  };
}


app.on = false;
// Vue.config.errorHandler = (err, vm, info) => {
//   console.error(err);
// };
let v = new Vue({
  el: '#app',
  data: app,
  // config: {
  //   errorHandler: function (err, vm, info) {
  //     console.error(err);
  //   },
  // },
  methods: {
    refresh() {
      v.$forceUpdate();
    },
    start: function () {
      this.game = new Game();
      this.on = true;
      this.refresh();
    },
    load: function () {
    },

    select: function (tile) {
      this.game.select(tile);
      this.refresh();
    },
    move: function (tile) {
      if (this.game.selected.player === this.game.player) {
        if (this.game.selected === tile) {
          this.game.selected.build();
        } else if (this.game.selected.distance(tile) === 1) {
          this.game.selected.move(tile);
          this.select(tile);
        }
      }
      this.refresh();
    },
    end: function () {
      this.game.end();
      this.refresh();
    },

    test: function () {
      // game.values.push(Math.random());
      // let avg = game.random.range(3, 21);
      // let dev = game.random.float();
      // let oe = game.random.float();
      // let cap = game.random.next(26);
      // game.values.push(avg); game.values.push(dev); game.values.push(oe); game.values.push(cap);
      // game.arr = [];
      // let min = 0;
      // let aa = 0;
      // for (let a = 0; a < 10000; a++) {
      //   let v = game.random.gaussianOEInt(avg, dev, oe, cap);
      //   game.arr[v] = (game.arr[v] || 0) + 1;
      //   if (v < min) min = v;
      //   aa += v;
      // }
      // aa /= 10000;
      // game.values.push(aa);
      // for (let a = min; a < game.arr.length; a++) {
      //   if (game.arr[a]) {
      //     game.values.push(a);
      //     game.values.push(game.arr[a]);
      //   }
      // }
      // game.values.push(game.random.float());
    },
  },
})
